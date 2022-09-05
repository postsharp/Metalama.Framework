// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Linking.Inlining;
using Metalama.Framework.Engine.Linking.Substitution;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        /// <summary>
        /// Generates all subsitutions required to get correct bodies for semantics during the linking step.
        /// </summary>
        private class SubstitutionGenerator
        {
            private readonly LinkerSyntaxHandler _syntaxHandler;
            private readonly LinkerIntroductionRegistry _introductionRegistry;
            private readonly IReadOnlyList<IntermediateSymbolSemantic> _nonInlinedSemantics;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> _nonInlinedReferences;
            private readonly IReadOnlyList<InliningSpecification> _inliningSpecifications;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult> _bodyAnalysisResults;

            public SubstitutionGenerator( 
                LinkerSyntaxHandler syntaxHandler,
                LinkerIntroductionRegistry introductionRegistry,
                IReadOnlyList<IntermediateSymbolSemantic> nonInlinedSemantics, 
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> nonInlinedReferences,
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, SemanticBodyAnalysisResult> bodyAnalysisResults,
                IReadOnlyList<InliningSpecification> inliningSpecifications )
            {
                this._syntaxHandler = syntaxHandler;
                this._introductionRegistry = introductionRegistry;
                this._nonInlinedSemantics = nonInlinedSemantics;
                this._nonInlinedReferences = nonInlinedReferences;
                this._inliningSpecifications = inliningSpecifications;
                this._bodyAnalysisResults = bodyAnalysisResults;
            }

            internal IReadOnlyDictionary<InliningContextIdentifier, IReadOnlyList<SyntaxNodeSubstitution>> Run()
            {
                var substitutions = new Dictionary<InliningContextIdentifier, Dictionary<SyntaxNode, SyntaxNodeSubstitution>>();

                // Add substitutions to non-inlined semantics (these are always roots of inlining).
                foreach ( var nonInlinedSemantic in this._nonInlinedSemantics )
                {
                    if (nonInlinedSemantic.Symbol is not IMethodSymbol)
                    {
                        // Skip non-body semantics.
                        continue;
                    }

                    var nonInlinedSemanticBody = nonInlinedSemantic.ToTyped<IMethodSymbol>();

                    switch ( nonInlinedSemanticBody.Kind )
                    {
                        case IntermediateSymbolSemanticKind.Default:
                            if (this._introductionRegistry.IsOverrideTarget( nonInlinedSemanticBody.Symbol) && nonInlinedSemanticBody.Kind == IntermediateSymbolSemanticKind.Final)
                            {
                                AddSubstitution(
                                    new InliningContextIdentifier( nonInlinedSemanticBody ),
                                    this.CreateLastOverrideSubstitution( nonInlinedSemanticBody.Symbol, this._introductionRegistry.GetLastOverride( nonInlinedSemanticBody.Symbol ) ) );
                            }
                            else if ( this._introductionRegistry.IsOverride( nonInlinedSemanticBody.Symbol ) )
                            {
                                if ( this._nonInlinedReferences.TryGetValue( nonInlinedSemanticBody, out var nonInlinedReferenceList ) )
                                {
                                    foreach ( var nonInlinedReference in nonInlinedReferenceList )
                                    {
                                        AddSubstitution( new InliningContextIdentifier( nonInlinedSemanticBody ), new AspectReferenceSubstitution( nonInlinedReference ) );
                                    }
                                }
                            }

                            break;

                        case IntermediateSymbolSemanticKind.Final:
                        case IntermediateSymbolSemanticKind.Base:
                            break;

                        default:
                            throw new AssertionFailedException();
                    }
                }

                // Add substitutions for all inlining specifications.
                foreach (var inliningSpecification in this._inliningSpecifications)
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

                // TODO: We convert this later back to the dictionary, but for debugging it's better to have dictionary here.
                return substitutions.ToDictionary( x => x.Key, x => x.Value.Values.ToReadOnlyList() );

                void AddSubstitution( InliningContextIdentifier inliningContextId, SyntaxNodeSubstitution substitution )
                {
                    if (!substitutions.TryGetValue( inliningContextId, out var dictionary))
                    {
                        substitutions[inliningContextId] = dictionary = new Dictionary<SyntaxNode, SyntaxNodeSubstitution>();
                    }

                    dictionary.Add(substitution.TargetNode, substitution);
                }
            }

            private SyntaxNodeSubstitution CreateLastOverrideSubstitution( IMethodSymbol method, IMethodSymbol lastOverride )
            {
                var root = this._syntaxHandler.GetCanonicalRootNode( method );

                switch ( method )
                {
                    case { MethodKind: MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation }:
                        return new MethodRedirectionSubstitution( root, lastOverride );
                    case { MethodKind: MethodKind.PropertyGet, AssociatedSymbol: IPropertySymbol }:
                        return new PropertyGetRedirectionSubstitution( root, (IPropertySymbol) lastOverride.AssociatedSymbol.AssertNotNull() );
                    case { MethodKind: MethodKind.PropertySet, AssociatedSymbol: IPropertySymbol }:
                        return new PropertySetRedirectionSubstitution( root, (IPropertySymbol) lastOverride.AssociatedSymbol.AssertNotNull() );
                    case { MethodKind: MethodKind.EventAdd, AssociatedSymbol: IEventSymbol }:
                        return new EventAddRedirectionSubstitution( root, (IEventSymbol) lastOverride.AssociatedSymbol.AssertNotNull() );
                    case { MethodKind: MethodKind.EventRemove, AssociatedSymbol: IEventSymbol }:
                        return new EventRemoveRedirectionSubstitution( root, (IEventSymbol) lastOverride.AssociatedSymbol.AssertNotNull() );
                    default:
                        throw new AssertionFailedException();
                }
            }
        }
    }
}