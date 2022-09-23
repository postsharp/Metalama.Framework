// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        /// <summary>
        /// Generates all substitutions required to get correct bodies for semantics during the linking step.
        /// </summary>
        private class SubstitutionGenerator
        {
            private readonly LinkerSyntaxHandler _syntaxHandler;
            private readonly IReadOnlyList<IntermediateSymbolSemantic> _nonInlinedSemantics;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> _nonInlinedReferences;
            private readonly IReadOnlyList<InliningSpecification> _inliningSpecifications;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult> _bodyAnalysisResults;
            private readonly ITaskScheduler _taskScheduler;

            public SubstitutionGenerator(
                IServiceProvider serviceProvider,
                LinkerSyntaxHandler syntaxHandler,
                IReadOnlyList<IntermediateSymbolSemantic> nonInlinedSemantics,
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> nonInlinedReferences,
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult> bodyAnalysisResults,
                IReadOnlyList<InliningSpecification> inliningSpecifications )
            {
                this._taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
                this._syntaxHandler = syntaxHandler;
                this._nonInlinedSemantics = nonInlinedSemantics;
                this._nonInlinedReferences = nonInlinedReferences;
                this._inliningSpecifications = inliningSpecifications;
                this._bodyAnalysisResults = bodyAnalysisResults;
            }

            public async Task<IReadOnlyDictionary<InliningContextIdentifier, IReadOnlyList<SyntaxNodeSubstitution>>> RunAsync(
                CancellationToken cancellationToken )
            {
                var substitutions = new ConcurrentDictionary<InliningContextIdentifier, ConcurrentDictionary<SyntaxNode, SyntaxNodeSubstitution>>();
                var inliningTargetNodes = this._inliningSpecifications.Select( x => (x.ParentContextIdentifier, x.ReplacedRootNode) ).ToHashSet();

                // Add substitutions to non-inlined semantics (these are always roots of inlining).
                void ProcessNonInlinedSemantic( IntermediateSymbolSemantic nonInlinedSemantic )
                {
                    if ( nonInlinedSemantic.Symbol is not IMethodSymbol )
                    {
                        // Skip non-body semantics.
                        return;
                    }

                    var nonInlinedSemanticBody = nonInlinedSemantic.ToTyped<IMethodSymbol>();

                    // Add aspect reference substitution for all aspect references.
                    if ( this._nonInlinedReferences.TryGetValue( nonInlinedSemanticBody, out var nonInlinedReferenceList ) )
                    {
                        foreach ( var nonInlinedReference in nonInlinedReferenceList )
                        {
                            AddSubstitution( new InliningContextIdentifier( nonInlinedSemanticBody ), new AspectReferenceSubstitution( nonInlinedReference ) );
                        }
                    }
                }

                await this._taskScheduler.RunInParallelAsync( this._nonInlinedSemantics, ProcessNonInlinedSemantic, cancellationToken );

                // Add substitutions for all inlining specifications.
                void ProcessInlinlingSpecification( InliningSpecification inliningSpecification )
                {
                    // Add the inlining substitution itself.
                    AddSubstitution( inliningSpecification.ParentContextIdentifier, new InliningSubstitution( inliningSpecification ) );

                    // If not simple inlining, add return statement substitutions.
                    if ( !inliningSpecification.UseSimpleInlining )
                    {
                        var bodyAnalysisResult = this._bodyAnalysisResults[inliningSpecification.TargetSemantic];

                        // Add substitutions of return statements contained in the inlined body.
                        foreach ( var returnStatementRecord in bodyAnalysisResult.ReturnStatements )
                        {
                            // If the return statement is target of inlining, we don't create substitution for it.
                            if ( inliningTargetNodes.Contains( (inliningSpecification.ContextIdentifier, returnStatementRecord.Key) ) )
                            {
                                continue;
                            }

                            var returnStatement = returnStatementRecord.Key;
                            var returnStatementProperties = returnStatementRecord.Value;

                            Invariant.AssertNot( !returnStatementProperties.FlowsToExitIfRewritten && inliningSpecification.ReturnLabelIdentifier == null );

                            AddSubstitution(
                                inliningSpecification.ContextIdentifier,
                                new ReturnStatementSubstitution(
                                    returnStatement,
                                    inliningSpecification.AspectReference.ContainingSemantic.Symbol,
                                    inliningSpecification.ReturnVariableIdentifier,
                                    inliningSpecification.ReturnLabelIdentifier ) );
                        }

                        if ( inliningSpecification.ReturnLabelIdentifier != null &&
                            this._bodyAnalysisResults.TryGetValue( inliningSpecification.TargetSemantic, out var bodyAnalysisResults ) && bodyAnalysisResults.RootBlockWithUsingLocal != null )
                        {
                            AddSubstitution(
                                inliningSpecification.ContextIdentifier,
                                new RootWithUsingLocalSubstitution( bodyAnalysisResults.RootBlockWithUsingLocal ) );
                        }
                    }

                    // Add substitution that transforms original non-block body into a statement.
                    if ( inliningSpecification.TargetSemantic.Kind == IntermediateSymbolSemanticKind.Default )
                    {
                        var symbol = inliningSpecification.TargetSemantic.Symbol;
                        var root = this._syntaxHandler.GetCanonicalRootNode( symbol );

                        if ( root is not StatementSyntax )
                        {
                            AddSubstitution(
                                inliningSpecification.ContextIdentifier,
                                CreateOriginalBodySubstitution( root, symbol, inliningSpecification.ReturnVariableIdentifier ) );
                        }
                    }

                    // Add substitutions of non-inlined aspect references.
                    if ( this._nonInlinedReferences.TryGetValue( inliningSpecification.TargetSemantic, out var nonInlinedReferenceList ) )
                    {
                        foreach ( var nonInlinedReference in nonInlinedReferenceList )
                        {
                            AddSubstitution( inliningSpecification.ContextIdentifier, new AspectReferenceSubstitution( nonInlinedReference ) );
                        }
                    }
                }

                await this._taskScheduler.RunInParallelAsync( this._inliningSpecifications, ProcessInlinlingSpecification, cancellationToken );

                // TODO: We convert this later back to the dictionary, but for debugging it's better to have dictionary also here.
                return substitutions.ToDictionary( x => x.Key, x => x.Value.Values.ToReadOnlyList() );

                void AddSubstitution( InliningContextIdentifier inliningContextId, SyntaxNodeSubstitution substitution )
                {
                    var dictionary = substitutions.GetOrAddNew( inliningContextId );

                    if ( !dictionary.TryAdd( substitution.TargetNode, substitution ) )
                    {
                        // TODO: The item was already added, but there is no logic to cover this situation.
                        throw new AssertionFailedException();
                    }
                }
            }

            private static SyntaxNodeSubstitution CreateOriginalBodySubstitution( SyntaxNode root, IMethodSymbol symbol, string? returnVariableIdentifier )
            {
                switch (root, symbol)
                {
                    case (AccessorDeclarationSyntax accessorDeclarationSyntax, { AssociatedSymbol: IPropertySymbol property }):
                        return new AutoPropertyAccessorSubstitution( accessorDeclarationSyntax, property, returnVariableIdentifier );

                    case (ArrowExpressionClauseSyntax arrowExpressionClause, _):
                        return new ExpressionBodySubstitution( arrowExpressionClause, symbol, returnVariableIdentifier );

                    case (VariableDeclaratorSyntax { Parent: { Parent: EventFieldDeclarationSyntax } } variableDeclarator, { AssociatedSymbol: IEventSymbol }):
                        Invariant.Assert( returnVariableIdentifier == null );

                        return new EventFieldSubstitution( variableDeclarator, symbol );

                    case (MethodDeclarationSyntax { Body: null, ExpressionBody: null } emptyVoidPartialMethod, _):
                        Invariant.Assert( returnVariableIdentifier == null );

                        return new EmptyVoidPartialMethodSubstitution( emptyVoidPartialMethod );

                    case (ParameterSyntax { Parent: ParameterListSyntax { Parent: RecordDeclarationSyntax } } recordParameter, _):
                        return new RecordParameterSubstitution( recordParameter, symbol, returnVariableIdentifier );

                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}