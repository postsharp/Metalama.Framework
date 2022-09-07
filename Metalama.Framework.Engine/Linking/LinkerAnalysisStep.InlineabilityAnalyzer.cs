// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Metalama.Framework.Engine.Linking
{
    internal partial class LinkerAnalysisStep
    {
        /// <summary>
        /// Analyzes inlineability of semantics and references. Determines which semantics and references will be inlined.
        /// </summary>
        public class InlineabilityAnalyzer
        {
            private readonly PartialCompilation _intermediateCompilation;
            private readonly LinkerIntroductionRegistry _introductionRegistry;
            private readonly ISet<IntermediateSymbolSemantic> _reachableSymbolSemantics;
            private readonly InlinerProvider _inlinerProvider;
            private readonly IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> _reachableReferencesBySource;
            private readonly IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> _reachableReferencesByTarget;

            public InlineabilityAnalyzer(
                PartialCompilation intermediateCompilation,
                LinkerIntroductionRegistry introductionRegistry,
                IReadOnlyList<IntermediateSymbolSemantic> reachableSymbolSemantics,
                InlinerProvider inlinerProvider,
                IReadOnlyDictionary<IntermediateSymbolSemantic<IMethodSymbol>, IReadOnlyList<ResolvedAspectReference>> reachableReferencesBySource,
                IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> reachableReferencesByTarget )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._introductionRegistry = introductionRegistry;
                this._reachableSymbolSemantics = new HashSet<IntermediateSymbolSemantic>( reachableSymbolSemantics );
                this._inlinerProvider = inlinerProvider;
                this._reachableReferencesBySource = reachableReferencesBySource;
                this._reachableReferencesByTarget = reachableReferencesByTarget;
            }

            private IReadOnlyList<ResolvedAspectReference> GetReachableReferencesByTarget(AspectReferenceTarget target)
            {
                if (!this._reachableReferencesByTarget.TryGetValue(target, out var references))
                {
                    return Array.Empty<ResolvedAspectReference>();
                }

                return references;
            }

            private IReadOnlyList<ResolvedAspectReference> GetReachableReferencesBySource( IntermediateSymbolSemantic<IMethodSymbol> source )
            {
                if ( !this._reachableReferencesBySource.TryGetValue( source, out var references ) )
                {
                    return Array.Empty<ResolvedAspectReference>();
                }

                return references;
            }

            /// <summary>
            /// Gets semantics that are inlineable, i.e. are not referenced too many times and don't have inlineability explicitly suppressed.
            /// </summary>
            /// <returns></returns>
            public IReadOnlyList<IntermediateSymbolSemantic> GetInlineableSemantics()
            {
                var inlineableSemantics = new List<IntermediateSymbolSemantic>();

                foreach (var semantic in this._reachableSymbolSemantics)
                {
                    if ( IsInlineable( semantic ) )
                    {
                        inlineableSemantics.Add( semantic );
                    }
                }

                return inlineableSemantics;

                bool IsInlineable(IntermediateSymbolSemantic semantic)
                {
                    if ( semantic.Symbol.GetDeclarationFlags().HasFlag( LinkerDeclarationFlags.NotInlineable ))
                    {
                        // Semantics marked as non-inlineable are not inlineable.
                        return false;
                    }

                    if ( semantic.Kind == IntermediateSymbolSemanticKind.Final || semantic.Kind == IntermediateSymbolSemanticKind.Base )
                    {
                        // Final and base semantic is never inlineable.
                        return false;
                    }

                    switch ( semantic.Symbol )
                    {
                        case IMethodSymbol:
                            return IsInlineableMethod( semantic.ToTyped<IMethodSymbol>() );

                        case IPropertySymbol:
                            return IsInlineableProperty( semantic.ToTyped<IPropertySymbol>() );

                        case IEventSymbol:
                            return IsInlineableEvent( semantic.ToTyped<IEventSymbol>() );

                        default:
                            throw new AssertionFailedException();
                    }
                }

                bool IsInlineableMethod( IntermediateSymbolSemantic<IMethodSymbol> semantic )
                {
                    switch ( semantic.Symbol.MethodKind )
                    {
                        case MethodKind.Ordinary:
                        case MethodKind.ExplicitInterfaceImplementation:
                        case MethodKind.Destructor:
                        case MethodKind.UserDefinedOperator:
                        case MethodKind.Conversion:
                            var aspectReferences = this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget() );

                            if ( aspectReferences.Count != 1 )
                            {
                                return false;
                            }
                            else
                            {
                                return true;
                            }

                        case MethodKind.EventAdd:
                        case MethodKind.EventRemove:
                        case MethodKind.PropertyGet:
                        case MethodKind.PropertySet:
                            // Accessor methods are inlineable by themselves, further conditions are evaluated under inlineability of property/event.
                            return true;

                        default:
                            throw new AssertionFailedException();
                    }
                }

                bool IsInlineableProperty( IntermediateSymbolSemantic<IPropertySymbol> semantic )
                {
                    var getAspectReferences = this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget( AspectReferenceTargetKind.PropertyGetAccessor ));
                    var setAspectReferences = this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget( AspectReferenceTargetKind.PropertySetAccessor ));

                    if ( getAspectReferences.Count > 1 || setAspectReferences.Count > 1
                                                       || (getAspectReferences.Count == 0 && setAspectReferences.Count == 0) )
                    {
                        return false;
                    }

                    return true;
                }

                bool IsInlineableEvent( IntermediateSymbolSemantic<IEventSymbol> semantic )
                {
                    var addAspectReferences = this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget( AspectReferenceTargetKind.EventAddAccessor ) );
                    var removeAspectReferences = this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget( AspectReferenceTargetKind.EventRemoveAccessor ) );

                    if ( addAspectReferences.Count > 1 || removeAspectReferences.Count > 1
                                                       || (addAspectReferences.Count == 0 && removeAspectReferences.Count == 0) )
                    {
                        return false;
                    }

                    return true;
                }
            }

            /// <summary>
            /// Determines which aspect references can be inlined.
            /// </summary>
            /// <param name="aspectReferences"></param>
            /// <returns>Aspect references with selected inliners.</returns>
            public IReadOnlyDictionary<ResolvedAspectReference, Inliner> GetInlineableReferences(IReadOnlyList<IntermediateSymbolSemantic> inlineableSemantics)
            {
                var inlineableReferences = new Dictionary<ResolvedAspectReference, Inliner>();

                foreach ( var inlineableSemantic in inlineableSemantics )
                {
                    foreach ( var reference in this.GetReachableReferencesByTarget( inlineableSemantic.ToAspectReferenceTarget() ) )
                    {
                        if ( IsInlineable( reference, out var inliner ) )
                        {
                            inlineableReferences.Add( reference, inliner );
                        }
                    }
                }

                return inlineableReferences;

                bool IsInlineable( ResolvedAspectReference reference, [NotNullWhen( true )] out Inliner? inliner )
                {
                    if ( !reference.IsInlineable )
                    {
                        inliner = null;
                        return false;
                    }

                    if (reference.SourceNode is not ExpressionSyntax)
                    {
                        inliner = ImplicitLastOverrideReferenceInliner.Instance;
                        return true;
                    }

                    var semanticModel = this._intermediateCompilation.Compilation.GetSemanticModel( reference.SourceExpression.SyntaxTree );

                    return this._inlinerProvider.TryGetInliner( reference, semanticModel, out inliner );
                }
            }

            /// <summary>
            /// Gets all semantics that are inlined.
            /// </summary>
            /// <param name="inlineableSemantics"></param>
            /// <param name="inlineableReferences"></param>
            /// <returns></returns>
            public IReadOnlyList<IntermediateSymbolSemantic> GetInlinedSemantics( IReadOnlyList<IntermediateSymbolSemantic> inlineableSemantics, IReadOnlyDictionary<ResolvedAspectReference, Inliner> inlineableReferences)
            {
                var inlineableSemanticHashSet = new HashSet<IntermediateSymbolSemantic>(inlineableSemantics);
                var inlinedSemantics = new List<IntermediateSymbolSemantic>();

                foreach (var inlineableSemantic in inlineableSemantics)
                {
                    if ( IsInlinedSemantic( inlineableSemantic ) )
                    {
                        inlinedSemantics.Add( inlineableSemantic );
                    }
                }

                return inlinedSemantics;

                bool IsInlinedSemanticBody(IntermediateSymbolSemantic<IMethodSymbol> semanticBody)
                {
                    if ( !this._reachableReferencesByTarget.TryGetValue( semanticBody.ToAspectReferenceTarget(), out var aspectReferences ) )
                    {
                        // This semantic does not have any incoming reference.
                        return false;
                    }

                    var anyNonInlineableReference = false;

                    foreach ( var reference in aspectReferences )
                    {
                        if ( !inlineableReferences.ContainsKey( reference ) )
                        {
                            anyNonInlineableReference = true;
                            break;
                        }
                    }

                    return !anyNonInlineableReference;
                }

                bool IsInlinedSemantic(IntermediateSymbolSemantic semantic)
                {
                    switch ( semantic.Symbol )
                    {
                        case IMethodSymbol { MethodKind: MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation or MethodKind.UserDefinedOperator or MethodKind.Conversion or MethodKind.Destructor } method:
                            return IsInlinedSemanticBody( semantic.ToTyped<IMethodSymbol>() );

                        case IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } propertyAccessor:
                            return IsInlinedSemantic( new IntermediateSymbolSemantic( propertyAccessor.AssociatedSymbol.AssertNotNull(), semantic.Kind ) );

                        case IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove } eventAccessor:
                            return IsInlinedSemantic( new IntermediateSymbolSemantic( eventAccessor.AssociatedSymbol.AssertNotNull(), semantic.Kind ) );

                        case IPropertySymbol property:
                            // Property is inlined if at least one of the accessors is reachable and not inlineable.
                            var hasNonInlinedGet =
                                property.GetMethod != null 
                                && !IsInlinedSemanticBody( new IntermediateSymbolSemantic<IMethodSymbol>( property.GetMethod, semantic.Kind ))
                                && this._reachableSymbolSemantics.Contains( new IntermediateSymbolSemantic<IMethodSymbol>( property.GetMethod, semantic.Kind ) );

                            var hasNonInlinedSet =
                                property.SetMethod != null 
                                && !IsInlinedSemanticBody( new IntermediateSymbolSemantic<IMethodSymbol>( property.SetMethod, semantic.Kind ) )
                                && this._reachableSymbolSemantics.Contains( new IntermediateSymbolSemantic<IMethodSymbol>( property.SetMethod, semantic.Kind ) );

                            return inlineableSemanticHashSet.Contains(semantic) && !hasNonInlinedGet && !hasNonInlinedSet;

                        case IEventSymbol @event:
                            // Event is inlined if at least one of the accessors is reachable and not inlineable.
                            var hasNonInlinedAdd = 
                                !IsInlinedSemanticBody( new IntermediateSymbolSemantic<IMethodSymbol>( @event.AddMethod.AssertNotNull(), semantic.Kind ) )
                                && this._reachableSymbolSemantics.Contains( new IntermediateSymbolSemantic<IMethodSymbol>( @event.AddMethod.AssertNotNull(), semantic.Kind ) );

                            var hasNonInlinedRemove = 
                                !IsInlinedSemanticBody( new IntermediateSymbolSemantic<IMethodSymbol>( @event.RemoveMethod.AssertNotNull(), semantic.Kind ) )
                                && this._reachableSymbolSemantics.Contains( new IntermediateSymbolSemantic<IMethodSymbol>( @event.RemoveMethod.AssertNotNull(), semantic.Kind ) );

                            return inlineableSemanticHashSet.Contains( semantic ) && !hasNonInlinedAdd && !hasNonInlinedRemove;

                        default:
                            throw new AssertionFailedException();
                    }
                }
            }

            /// <summary>
            /// Gets all references that are inlined.
            /// </summary>
            /// <param name="inlineableReferences"></param>
            /// <param name="inlinedSemantics"></param>
            /// <returns></returns>
            public IReadOnlyDictionary<ResolvedAspectReference, Inliner> GetInlinedReferences( IReadOnlyDictionary<ResolvedAspectReference, Inliner> inlineableReferences, IReadOnlyList<IntermediateSymbolSemantic> inlinedSemantics)
            {
                var inlinedReferences = new Dictionary<ResolvedAspectReference, Inliner>();

                foreach ( var inlinedSemantic in inlinedSemantics )
                {
                    foreach ( var reference in this.GetReachableReferencesByTarget( inlinedSemantic.ToAspectReferenceTarget() ) )
                    {
                        if (!inlineableReferences.TryGetValue(reference, out var inliner))
                        {
                            throw new AssertionFailedException();
                        }

                        inlinedReferences.Add( reference, inliner );
                    }
                }

                return inlinedReferences;
            }
        }
    }
}