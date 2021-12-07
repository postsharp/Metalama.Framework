// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Impl.Linking
{
    internal partial class LinkerAnalysisStep
    {
        private class ReachabilityAnalyzer
        {
            private readonly LinkerIntroductionRegistry _introductionRegistry;
            private readonly IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> _methodBodyAnalysisResults;

            public ReachabilityAnalyzer(
                LinkerIntroductionRegistry introductionRegistry,
                IReadOnlyDictionary<ISymbol, MethodBodyAnalysisResult> methodBodyAnalysisResults )
            {
                this._introductionRegistry = introductionRegistry;
                this._methodBodyAnalysisResults = methodBodyAnalysisResults;
            }

            public IReadOnlyList<IntermediateSymbolSemantic> AnalyzeReachability()
            {
                // TODO: Optimize (should not allocate closures).
                // TODO: Is using call stack reliable enough?
                var visited = new HashSet<IntermediateSymbolSemantic>();

                // Assume G(V, E) is a graph where vertices V are semantics of overridden declarations and overrides.
                // Determine which semantics are reachable from final semantics using DFS.                

                // Run DFS from each overridden member's final semantic.
                foreach ( var overriddenMember in this._introductionRegistry.GetOverriddenMembers() )
                {
                    DepthFirstSearch( new IntermediateSymbolSemantic( overriddenMember, IntermediateSymbolSemanticKind.Final ) );
                }

                // Run DFS from any non-discardable declaration
                foreach ( var introducedMember in this._introductionRegistry.GetIntroducedMembers() )
                {
                    if ( introducedMember.Syntax.GetLinkerDeclarationFlags().HasFlag( LinkerDeclarationFlags.NotDiscardable ) )
                    {
                        DepthFirstSearch(
                            new IntermediateSymbolSemantic(
                                this._introductionRegistry.GetSymbolForIntroducedMember( introducedMember ),
                                IntermediateSymbolSemanticKind.Default ) );
                    }
                }

                return visited.ToList();

                void DepthFirstSearch( IntermediateSymbolSemantic current )
                {
                    // TODO: Some edges we are walking are not necessary and may hinder performance.
                    if ( !visited.Add( current ) )
                    {
                        return;
                    }

                    // Edges between accessors and method group.
                    switch ( current.Symbol )
                    {
                        case IMethodSymbol method:
                            if ( method.AssociatedSymbol != null )
                            {
                                DepthFirstSearch( new IntermediateSymbolSemantic( method.AssociatedSymbol, current.Kind ) );
                            }

                            break;

                        case IPropertySymbol property:
                            if ( property.GetMethod != null )
                            {
                                DepthFirstSearch( new IntermediateSymbolSemantic( property.GetMethod, current.Kind ) );
                            }

                            if ( property.SetMethod != null )
                            {
                                DepthFirstSearch( new IntermediateSymbolSemantic( property.SetMethod, current.Kind ) );
                            }

                            break;

                        case IEventSymbol @event:
                            DepthFirstSearch( new IntermediateSymbolSemantic( @event.AddMethod.AssertNotNull(), current.Kind ) );
                            DepthFirstSearch( new IntermediateSymbolSemantic( @event.RemoveMethod.AssertNotNull(), current.Kind ) );

                            break;

                        case IFieldSymbol:
                            break;

                        default:
                            throw new AssertionFailedException();
                    }

                    if ( this._introductionRegistry.IsOverrideTarget( current.Symbol ) )
                    {
                        if ( current.Kind == IntermediateSymbolSemanticKind.Final )
                        {
                            // Edge representing the implicit reference from final semantic to last override symbol.
                            DepthFirstSearch(
                                new IntermediateSymbolSemantic(
                                    this._introductionRegistry.GetLastOverride( current.Symbol ),
                                    IntermediateSymbolSemanticKind.Default ) );
                        }

                        // If the semantic is not final (original or base), there is nothing to do.
                    }
                    else
                    {
                        // Only method symbols with analysis results are taken into account.
                        if ( current.Symbol is IMethodSymbol
                             && this._methodBodyAnalysisResults.TryGetValue( current.Symbol, out var analysisResult ) )
                        {
                            // Edges representing resolved aspect references.
                            foreach ( var aspectReference in analysisResult.AspectReferences )
                            {
                                DepthFirstSearch( aspectReference.ResolvedSemantic );
                            }
                        }
                    }
                }
            }
        }
    }
}