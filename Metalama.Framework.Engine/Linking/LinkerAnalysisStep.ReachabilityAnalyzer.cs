// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Services;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.Linking
{
    internal sealed partial class LinkerAnalysisStep
    {
        private sealed class ReachabilityAnalyzer
        {
            private readonly CompilationContext _intermediateCompilationContext;
            private readonly LinkerInjectionRegistry _injectionRegistry;

            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyCollection<ResolvedAspectReference>>
                _aspectReferencesBySemantic;

            private readonly IReadOnlyList<IntermediateSymbolSemantic> _additionalNonDiscardableSemantics;

            public ReachabilityAnalyzer(
                CompilationContext intermediateCompilationContext,
                LinkerInjectionRegistry injectionRegistry,
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyCollection<ResolvedAspectReference>> aspectReferencesBySemantic,
                IReadOnlyList<IntermediateSymbolSemantic> additionalNonDiscardableSemantics )
            {
                this._intermediateCompilationContext = intermediateCompilationContext;
                this._injectionRegistry = injectionRegistry;
                this._aspectReferencesBySemantic = aspectReferencesBySemantic;
                this._additionalNonDiscardableSemantics = additionalNonDiscardableSemantics;
            }

            public HashSet<IntermediateSymbolSemantic> Run()
            {
                // TODO: Optimize (should not allocate closures).
                // TODO: Is using call stack reliable enough?
                var visited =
                    new HashSet<IntermediateSymbolSemantic>(
                        IntermediateSymbolSemanticEqualityComparer.ForCompilation( this._intermediateCompilationContext ) );

                // Assume G(V, E) is a graph where vertices V are semantics of overridden declarations and overrides.
                // Determine which semantics are reachable from final semantics using DFS.                

                // Run DFS from each overridden member's final semantic.
                foreach ( var overriddenMember in this._injectionRegistry.GetOverriddenMembers() )
                {
                    switch ( overriddenMember )
                    {
                        case IMethodSymbol method:
                            DepthFirstSearch( method.ToSemantic( IntermediateSymbolSemanticKind.Final ) );

                            break;

                        case IPropertySymbol property:
                            if ( property.GetMethod != null )
                            {
                                DepthFirstSearch( property.GetMethod.ToSemantic( IntermediateSymbolSemanticKind.Final ) );
                            }

                            if ( property.SetMethod != null )
                            {
                                DepthFirstSearch( property.SetMethod.ToSemantic( IntermediateSymbolSemanticKind.Final ) );
                            }
                            else if ( property is { SetMethod: null, OverriddenProperty: not null } && property.IsAutoProperty().GetValueOrDefault() )
                            {
                                // For auto-properties that override a property without a setter, the first override needs to be implicitly reachable.
                                var lastOverrideSetter = ((IPropertySymbol) this._injectionRegistry.GetLastOverride( property ).AssertNotNull()).SetMethod
                                    .AssertNotNull();

                                DepthFirstSearch( lastOverrideSetter.ToSemantic( IntermediateSymbolSemanticKind.Default ) );
                            }

                            break;

                        case IEventSymbol @event:
                            DepthFirstSearch( @event.AddMethod.AssertNotNull().ToSemantic( IntermediateSymbolSemanticKind.Final ) );
                            DepthFirstSearch( @event.RemoveMethod.AssertNotNull().ToSemantic( IntermediateSymbolSemanticKind.Final ) );

                            break;
                    }
                }

                // Run DFS from any non-discardable declaration.
                foreach ( var injectedMember in this._injectionRegistry.GetInjectedMembers() )
                {
                    if ( injectedMember.Syntax.GetLinkerDeclarationFlags().HasFlagFast( AspectLinkerDeclarationFlags.NotDiscardable ) )
                    {
                        switch ( this._injectionRegistry.GetSymbolForInjectedMember( injectedMember ) )
                        {
                            case IMethodSymbol method:
                                DepthFirstSearch( method.ToSemantic( IntermediateSymbolSemanticKind.Default ) );

                                break;

                            case IPropertySymbol property:
                                if ( property.GetMethod != null )
                                {
                                    DepthFirstSearch( property.GetMethod.ToSemantic( IntermediateSymbolSemanticKind.Default ) );
                                }

                                if ( property.SetMethod != null )
                                {
                                    DepthFirstSearch( property.SetMethod.ToSemantic( IntermediateSymbolSemanticKind.Default ) );
                                }

                                break;

                            case IEventSymbol @event:
                                DepthFirstSearch( @event.AddMethod.AssertNotNull().ToSemantic( IntermediateSymbolSemanticKind.Default ) );
                                DepthFirstSearch( @event.RemoveMethod.AssertNotNull().ToSemantic( IntermediateSymbolSemanticKind.Default ) );

                                break;
                        }
                    }
                }

                // Run DFS for additional non-discardable semantics
                foreach ( var semantic in this._additionalNonDiscardableSemantics )
                {
                    DepthFirstSearch( semantic );
                }

                return visited;

                void DepthFirstSearch( IntermediateSymbolSemantic current )
                {
                    // TODO: Some edges we are walking are not necessary and may hinder performance.
                    if ( !visited.Add( current ) )
                    {
                        return;
                    }

                    // Implicit edges between accessors and method group.
                    switch ( current.Symbol )
                    {
                        case IMethodSymbol { AssociatedSymbol: IPropertySymbol property }:
                            DepthFirstSearch( new IntermediateSymbolSemantic( property, current.Kind ) );

                            break;

                        case IMethodSymbol { AssociatedSymbol: IEventSymbol @event }:
                            DepthFirstSearch( new IntermediateSymbolSemantic( @event, current.Kind ) );

                            break;

                        case IMethodSymbol { AssociatedSymbol: null }:
                        case IPropertySymbol:
                        case IEventSymbol:
                        case IFieldSymbol:
                            // Do nothing on method groups and fields as these do not have implicit references.
                            break;

                        default:
                            throw new AssertionFailedException( $"Unexpected symbol: '{current.Symbol}'" );
                    }

                    // If the method contains aspect references, visit them.
                    if ( current.Symbol is IMethodSymbol
                         && this._aspectReferencesBySemantic.TryGetValue( current.ToTyped<IMethodSymbol>(), out var aspectReferences ) )
                    {
                        // Edges representing resolved aspect references.
                        foreach ( var aspectReference in aspectReferences )
                        {
                            if ( !this._intermediateCompilationContext.SymbolComparer.Equals(
                                    current.Symbol.ContainingType,
                                    aspectReference.ResolvedSemantic.Symbol.ContainingType ) )
                            {
                                // Symbols declared in other types are not reachable.
                                continue;
                            }

                            if ( aspectReference.HasResolvedSemanticBody )
                            {
                                DepthFirstSearch( aspectReference.ResolvedSemanticBody );
                            }
                            else
                            {
                                // If the semantic does not have a body, visit at least the semantic (case for fields).
                                DepthFirstSearch( aspectReference.ResolvedSemantic );
                            }
                        }
                    }
                }
            }
        }
    }
}