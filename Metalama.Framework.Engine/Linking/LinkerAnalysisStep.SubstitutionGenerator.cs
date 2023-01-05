﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Linking.Substitution;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Roslyn;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerAnalysisStep
    {
        /// <summary>
        /// Generates all substitutions required to get correct bodies for semantics during the linking step.
        /// </summary>
        private sealed class SubstitutionGenerator
        {
            private readonly LinkerSyntaxHandler _syntaxHandler;
            private readonly HashSet<IntermediateSymbolSemantic> _inlinedSemantics;
            private readonly IReadOnlyList<IntermediateSymbolSemantic> _nonInlinedSemantics;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> _nonInlinedReferences;
            private readonly IReadOnlyList<InliningSpecification> _inliningSpecifications;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult> _bodyAnalysisResults;
            private readonly IReadOnlyDictionary<ISymbol, IntermediateSymbolSemantic> _redirectedSymbols;
            private readonly IReadOnlyList<IntermediateSymbolSemantic> _additionalTransformedSemantics;
            private readonly IReadOnlyList<ForcefullyInitializedType> _forcefullyInitializedTypes;

            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<IntermediateSymbolSemanticReference>>
                _redirectedSymbolReferencesByContainingSemantic;

            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<IntermediateSymbolSemanticReference>>
                _eventFieldRaiseReferencesByContainingSemantic;

            private readonly ITaskScheduler _taskScheduler;

            public SubstitutionGenerator(
                ProjectServiceProvider serviceProvider,
                LinkerSyntaxHandler syntaxHandler,
                IReadOnlyList<IntermediateSymbolSemantic> inlinedSemantics,
                IReadOnlyList<IntermediateSymbolSemantic> nonInlinedSemantics,
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> nonInlinedReferences,
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult> bodyAnalysisResults,
                IReadOnlyList<InliningSpecification> inliningSpecifications,
                IReadOnlyDictionary<ISymbol, IntermediateSymbolSemantic> redirectedSymbols,
                IReadOnlyList<IntermediateSymbolSemanticReference> redirectedSymbolReferences,
                IReadOnlyList<ForcefullyInitializedType> forcefullyInitializedTypes,
                IReadOnlyList<IntermediateSymbolSemanticReference> eventFieldRaiseReferences )
            {
                this._taskScheduler = serviceProvider.GetRequiredService<ITaskScheduler>();
                this._syntaxHandler = syntaxHandler;
                this._inlinedSemantics = new HashSet<IntermediateSymbolSemantic>( inlinedSemantics );
                this._nonInlinedSemantics = nonInlinedSemantics;
                this._nonInlinedReferences = nonInlinedReferences;
                this._inliningSpecifications = inliningSpecifications;
                this._bodyAnalysisResults = bodyAnalysisResults;
                this._redirectedSymbols = redirectedSymbols;
                this._forcefullyInitializedTypes = forcefullyInitializedTypes;

                this._additionalTransformedSemantics =
                    redirectedSymbolReferences.SelectAsEnumerable( x => (IntermediateSymbolSemantic) x.ContainingSemantic )
                        .Union( eventFieldRaiseReferences.SelectAsEnumerable( x => (IntermediateSymbolSemantic) x.ContainingSemantic ) )
                        .Except( inlinedSemantics )
                        .Distinct()
                        .ToList();

                this._redirectedSymbolReferencesByContainingSemantic = IndexReferenceByContainingBody( redirectedSymbolReferences );
                this._eventFieldRaiseReferencesByContainingSemantic = IndexReferenceByContainingBody( eventFieldRaiseReferences );

                static IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<IntermediateSymbolSemanticReference>>
                    IndexReferenceByContainingBody( IReadOnlyList<IntermediateSymbolSemanticReference> references )
                {
                    var dict = new Dictionary<IntermediateSymbolSemantic<IMethodSymbol>, List<IntermediateSymbolSemanticReference>>();

                    foreach ( var redirectedSymbolReference in references )
                    {
                        if ( !dict.TryGetValue( redirectedSymbolReference.ContainingSemantic, out var list ) )
                        {
                            dict[redirectedSymbolReference.ContainingSemantic] = list = new List<IntermediateSymbolSemanticReference>();
                        }

                        list.Add( redirectedSymbolReference );
                    }

                    return dict.ToDictionary( x => x.Key, x => (IReadOnlyList<IntermediateSymbolSemanticReference>) x.Value );
                }
            }

            public async Task<IReadOnlyDictionary<InliningContextIdentifier, IReadOnlyList<SyntaxNodeSubstitution>>> RunAsync(
                CancellationToken cancellationToken )
            {
                var substitutions = new ConcurrentDictionary<InliningContextIdentifier, ConcurrentDictionary<SyntaxNode, SyntaxNodeSubstitution>>();
                var inliningTargetNodes = this._inliningSpecifications.SelectAsEnumerable( x => (x.ParentContextIdentifier, x.ReplacedRootNode) ).ToHashSet();

                // Add substitutions to non-inlined semantics (these are always roots of inlining).
                void ProcessNonInlinedSemantic( IntermediateSymbolSemantic nonInlinedSemantic )
                {
                    if ( nonInlinedSemantic.Symbol is not IMethodSymbol )
                    {
                        // Skip non-body semantics.
                        return;
                    }

                    var nonInlinedSemanticBody = nonInlinedSemantic.ToTyped<IMethodSymbol>();
                    var context = new InliningContextIdentifier( nonInlinedSemanticBody );

                    // Add aspect reference substitution for all aspect references.
                    if ( this._nonInlinedReferences.TryGetValue( nonInlinedSemanticBody, out var nonInlinedReferenceList ) )
                    {
                        foreach ( var nonInlinedReference in nonInlinedReferenceList )
                        {
                            switch ( nonInlinedReference.OriginalSymbol )
                            {
                                case IPropertySymbol { Parameters.Length: > 0 }:
                                    // Indexers (and in future constructors), adds aspect parameter to the target.
                                    AddSubstitution( context, new AspectReferenceParameterSubstitution( nonInlinedReference ) );

                                    break;

                                default:
                                    // Everything else, renames the target.
                                    AddSubstitution( context, new AspectReferenceRenamingSubstitution( nonInlinedReference ) );

                                    break;
                            }
                        }
                    }

                    // Add substitutions for redirected nodes.
                    if ( this._redirectedSymbolReferencesByContainingSemantic.TryGetValue( nonInlinedSemanticBody, out var redirectedSymbolReference ) )
                    {
                        foreach ( var reference in redirectedSymbolReference )
                        {
                            var redirectionTarget = this._redirectedSymbols[reference.TargetSemantic.Symbol];

                            AddSubstitution( context, new RedirectionSubstitution( reference.ReferencingNode, redirectionTarget ) );
                        }
                    }

                    // Add substitutions for event field invocation references.
                    if ( this._eventFieldRaiseReferencesByContainingSemantic.TryGetValue( nonInlinedSemanticBody, out var eventFieldRaiseReferences ) )
                    {
                        foreach ( var reference in eventFieldRaiseReferences )
                        {
                            AddSubstitution(
                                context,
                                new EventFieldRaiseSubstitution(
                                    reference.ReferencingNode,
                                    (IEventSymbol) reference.TargetSemantic.Symbol,
                                    this._inlinedSemantics.Contains( reference.TargetSemantic ) ) );
                        }
                    }
                }

                await this._taskScheduler.RunInParallelAsync(
                    this._nonInlinedSemantics.Union( this._additionalTransformedSemantics ),
                    ProcessNonInlinedSemantic,
                    cancellationToken );

                // Add substitutions for all inlining specifications.
                void ProcessInliningSpecification( InliningSpecification inliningSpecification )
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
                                    inliningSpecification.AspectReference.ContainingBody,
                                    inliningSpecification.ReturnVariableIdentifier,
                                    inliningSpecification.ReturnLabelIdentifier,
                                    returnStatementProperties.ReplaceWithBreakIfOmitted ) );
                        }

                        if ( inliningSpecification.ReturnLabelIdentifier != null &&
                             this._bodyAnalysisResults.TryGetValue( inliningSpecification.TargetSemantic, out var bodyAnalysisResults ) )
                        {
                            // Add substitutions for blocks with using <type> <local>, which needs to be transformed into using statement.
                            foreach ( var block in bodyAnalysisResults.BlocksWithReturnBeforeUsingLocal )
                            {
                                AddSubstitution(
                                    inliningSpecification.ContextIdentifier,
                                    new BlockWithReturnBeforeUsingLocalSubstitution( block ) );
                            }
                        }
                    }

                    // Add substitution that transforms original non-block body into a statement.
                    if ( inliningSpecification.TargetSemantic.Kind == IntermediateSymbolSemanticKind.Default )
                    {
                        var referencedSymbol = inliningSpecification.TargetSemantic.Symbol;
                        var root = this._syntaxHandler.GetCanonicalRootNode( referencedSymbol );

                        switch ( root )
                        {
                            case not StatementSyntax:
                                AddSubstitution(
                                    inliningSpecification.ContextIdentifier,
                                    CreateOriginalBodySubstitution( 
                                        root,
                                        inliningSpecification.AspectReference.ContainingBody,
                                        referencedSymbol,
                                        inliningSpecification.UseSimpleInlining,
                                        inliningSpecification.ReturnVariableIdentifier ) );

                                break;

                            case BlockSyntax { Parent: AccessorDeclarationSyntax { Parent.Parent: EventDeclarationSyntax eventDeclaration } }
                                when eventDeclaration.GetLinkerDeclarationFlags().HasAnyFlagFast( AspectLinkerDeclarationFlags.HasHiddenInitializerExpression ):
                                // The event field has hidden initializer expression annotation.
                                // This means that the expression is hidden in the body of the accessor and the whole accessor body needs to be replaced.

                                AddSubstitution(
                                    inliningSpecification.ContextIdentifier,
                                    new EventFieldSubstitution( root, referencedSymbol ) );
                                break;
                                
                        }
                    }

                    // Add substitutions of non-inlined aspect references.
                    if ( this._nonInlinedReferences.TryGetValue( inliningSpecification.TargetSemantic, out var nonInlinedReferenceList ) )
                    {
                        foreach ( var nonInlinedReference in nonInlinedReferenceList )
                        {
                            switch ( inliningSpecification.AspectReference.OriginalSymbol )
                            {
                                case IPropertySymbol { Parameters.Length: > 0 }:
                                    // Indexers (and in future constructors), adds aspect parameter to the target.
                                    AddSubstitution( inliningSpecification.ContextIdentifier, new AspectReferenceParameterSubstitution( nonInlinedReference ) );

                                    break;

                                default:
                                    // Everything else, renames the target.
                                    AddSubstitution( inliningSpecification.ContextIdentifier, new AspectReferenceRenamingSubstitution( nonInlinedReference ) );

                                    break;
                            }
                        }
                    }

                    // Add substitutions for redirected nodes.
                    if ( this._redirectedSymbolReferencesByContainingSemantic.TryGetValue( inliningSpecification.TargetSemantic, out var references ) )
                    {
                        foreach ( var reference in references )
                        {
                            var redirectionTarget = this._redirectedSymbols[reference.TargetSemantic.Symbol];

                            AddSubstitution(
                                inliningSpecification.ContextIdentifier,
                                new RedirectionSubstitution( reference.ReferencingNode, redirectionTarget ) );
                        }
                    }

                    // Add substitutions for event field invocation references.
                    if ( this._eventFieldRaiseReferencesByContainingSemantic.TryGetValue(
                            inliningSpecification.TargetSemantic,
                            out var eventFieldRaiseReferences ) )
                    {
                        foreach ( var reference in eventFieldRaiseReferences )
                        {
                            AddSubstitution(
                                inliningSpecification.ContextIdentifier,
                                new EventFieldRaiseSubstitution(
                                    reference.ReferencingNode,
                                    (IEventSymbol) reference.TargetSemantic.Symbol,
                                    this._inlinedSemantics.Contains( reference.TargetSemantic ) ) );
                        }
                    }
                }

                await this._taskScheduler.RunInParallelAsync( this._inliningSpecifications, ProcessInliningSpecification, cancellationToken );

                void ProcessForcefullyInitializedType( ForcefullyInitializedType forcefullyInitializedType )
                {
                    foreach ( var constructor in forcefullyInitializedType.Constructors )
                    {
                        var context = new InliningContextIdentifier( constructor );

                        var declaration = (ConstructorDeclarationSyntax?) constructor.Symbol.GetPrimaryDeclaration();

                        if ( declaration == null )
                        {
                            // Skip implicit constructors. If needed, the constructor will be forced to be declared in the injection step.
                            continue;
                        }

                        var rootNode = declaration.Body ?? (SyntaxNode?) declaration.ExpressionBody
                            ?? throw new AssertionFailedException( "Declaration without body." );

                        AddSubstitution( context, new ForcedInitializationSubstitution( rootNode, forcefullyInitializedType.InitializedSymbols ) );
                    }
                }

                await this._taskScheduler.RunInParallelAsync( this._forcefullyInitializedTypes, ProcessForcefullyInitializedType, cancellationToken );

                // TODO: We convert this later back to the dictionary, but for debugging it's better to have dictionary also here.
                return substitutions.ToDictionary( x => x.Key, x => x.Value.Values.ToReadOnlyList() );

                void AddSubstitution( InliningContextIdentifier inliningContextId, SyntaxNodeSubstitution substitution )
                {
                    var dictionary = substitutions.GetOrAddNew( inliningContextId );

                    if ( !dictionary.TryAdd( substitution.TargetNode, substitution ) )
                    {
                        // TODO: The item was already added, but there is no logic to cover this situation.
                        throw new AssertionFailedException( $"The substitution was already added for node at '{substitution.TargetNode.GetLocation()}'." );
                    }
                }
            }

            private static SyntaxNodeSubstitution CreateOriginalBodySubstitution( SyntaxNode root, IMethodSymbol referencingSymbol, IMethodSymbol targetSymbol, bool usingSimpleInlining, string? returnVariableIdentifier )
            {
                switch (root, targetSymbol)
                {
                    case (AccessorDeclarationSyntax accessorDeclarationSyntax, { AssociatedSymbol: IPropertySymbol property }):
                        return new AutoPropertyAccessorSubstitution( accessorDeclarationSyntax, property, returnVariableIdentifier );

                    case (ArrowExpressionClauseSyntax arrowExpressionClause, _):
                        return new ExpressionBodySubstitution( arrowExpressionClause, referencingSymbol, targetSymbol, usingSimpleInlining, returnVariableIdentifier );

                    case (VariableDeclaratorSyntax { Parent.Parent: EventFieldDeclarationSyntax } variableDeclarator, { AssociatedSymbol: IEventSymbol }):
                        Invariant.Assert( returnVariableIdentifier == null );

                        return new EventFieldSubstitution( variableDeclarator, targetSymbol );

                    case (MethodDeclarationSyntax { Body: null, ExpressionBody: null } emptyVoidPartialMethod, _):
                        Invariant.Assert( returnVariableIdentifier == null );

                        return new EmptyVoidPartialMethodSubstitution( emptyVoidPartialMethod );

                    case (ParameterSyntax { Parent: ParameterListSyntax { Parent: RecordDeclarationSyntax } } recordParameter, _):
                        return new RecordParameterSubstitution( recordParameter, targetSymbol, returnVariableIdentifier );

                    default:
                        throw new AssertionFailedException( $"Unexpected combination: ('{root.GetLocation()}', '{targetSymbol}')." );
                }
            }
        }
    }
}