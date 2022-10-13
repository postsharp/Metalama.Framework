// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Linking.Inlining;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

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
            private readonly IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> _reachableReferencesByTarget;

            public InlineabilityAnalyzer(
                PartialCompilation intermediateCompilation,
                LinkerIntroductionRegistry introductionRegistry,
                IReadOnlyList<IntermediateSymbolSemantic> reachableSymbolSemantics,
                InlinerProvider inlinerProvider,
                IReadOnlyDictionary<AspectReferenceTarget, IReadOnlyList<ResolvedAspectReference>> reachableReferencesByTarget )
            {
                this._intermediateCompilation = intermediateCompilation;
                this._reachableSymbolSemantics = new HashSet<IntermediateSymbolSemantic>( reachableSymbolSemantics );
                this._inlinerProvider = inlinerProvider;
                this._reachableReferencesByTarget = reachableReferencesByTarget;
                this._introductionRegistry = introductionRegistry;
            }

            private IReadOnlyList<ResolvedAspectReference> GetReachableReferencesByTarget( AspectReferenceTarget target )
            {
                if ( !this._reachableReferencesByTarget.TryGetValue( target, out var references ) )
                {
                    return Array.Empty<ResolvedAspectReference>();
                }

                return references;
            }

            /// <summary>
            /// Gets semantics that are inlineable, i.e. are not referenced too many times and don't have inlineability explicitly suppressed.
            /// </summary>
            /// <returns></returns>
            public IReadOnlyList<IntermediateSymbolSemantic> GetInlineableSemantics( IReadOnlyDictionary<ISymbol, IntermediateSymbolSemantic> redirectedSymbols )
            {
                var redirectionTargets = new HashSet<IntermediateSymbolSemantic>( redirectedSymbols.Values );
                var inlineableSemantics = new List<IntermediateSymbolSemantic>();

                foreach ( var semantic in this._reachableSymbolSemantics )
                {
                    if ( IsInlineable( semantic ) )
                    {
                        inlineableSemantics.Add( semantic );
                    }
                }

                return inlineableSemantics;

                bool IsInlineable( IntermediateSymbolSemantic semantic )
                {
                    if ( semantic.Symbol.GetDeclarationFlags().HasFlag( AspectLinkerDeclarationFlags.NotInlineable ) )
                    {
                        // Semantics marked as non-inlineable are not inlineable.
                        return false;
                    }

                    if ( redirectionTargets.Contains(semantic) )
                    {
                        // Redirection targets are not inlineable.
                        return false;
                    }

                    if ( semantic.Kind == IntermediateSymbolSemanticKind.Final )
                    {
                        // Final semantic is never inlineable.
                        return false;
                    }

                    switch ( semantic.Symbol, semantic.Target )
                    {
                        case (IMethodSymbol, _):
                            return IsInlineableMethod( semantic.ToTyped<IMethodSymbol>() );

                        case (IPropertySymbol, not IntermediateSymbolSemanticTargetKind.Self):
                            return IsInlineableMethod( semantic.ToTyped<IMethodSymbol>() );

                        case (IEventSymbol, not IntermediateSymbolSemanticTargetKind.Self):
                            return IsInlineableMethod( semantic.ToTyped<IMethodSymbol>() );

                        case (IPropertySymbol, IntermediateSymbolSemanticTargetKind.Self):
                            return IsInlineableProperty( semantic.ToTyped<IPropertySymbol>() );

                        case (IEventSymbol, IntermediateSymbolSemanticTargetKind.Self):
                            return IsInlineableEvent( semantic.ToTyped<IEventSymbol>() );

                        case (IFieldSymbol, _):
                            // Fields are never inlineable.
                            return false;

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
                    var getAspectReferences =
                        this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget( AspectReferenceTargetKind.PropertyGetAccessor ) );

                    var setAspectReferences =
                        this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget( AspectReferenceTargetKind.PropertySetAccessor ) );

                    if ( getAspectReferences.Count > 1 || setAspectReferences.Count > 1
                                                       || (getAspectReferences.Count == 0 && setAspectReferences.Count == 0) )
                    {
                        return false;
                    }

                    return true;
                }

                bool IsInlineableEvent( IntermediateSymbolSemantic<IEventSymbol> semantic )
                {
                    var addAspectReferences =
                        this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget( AspectReferenceTargetKind.EventAddAccessor ) );

                    var removeAspectReferences =
                        this.GetReachableReferencesByTarget( semantic.ToAspectReferenceTarget( AspectReferenceTargetKind.EventRemoveAccessor ) );

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
            /// <returns>Aspect references with selected inliners.</returns>
            public IReadOnlyDictionary<ResolvedAspectReference, Inliner> GetInlineableReferences(
                IReadOnlyList<IntermediateSymbolSemantic> inlineableSemantics )
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
                    if ( reference.ContainingSemantic.Symbol.GetDeclarationFlags().HasFlag( AspectLinkerDeclarationFlags.NotInliningDestination ) )
                    {
                        // If containing semantic is marked as not being destination of inlining, the reference is not inlineable.
                        inliner = null;

                        return false;
                    }

                    if ( !reference.IsInlineable )
                    {
                        // References that are not marked as inlineable cannot be inlined.
                        inliner = null;

                        return false;
                    }

                    if ( !SymbolEqualityComparer.Default.Equals(
                            reference.ContainingSemantic.Symbol.ContainingType,
                            reference.ResolvedSemantic.Symbol.ContainingType ) )
                    {
                        // References between types cannot be inlined.
                        inliner = null;

                        return false;
                    }

                    if ( reference.SymbolSourceNode is not ExpressionSyntax )
                    {
                        // Use a special inliner for non-expression references.
                        inliner = ImplicitLastOverrideReferenceInliner.Instance;

                        return true;
                    }

                    var semanticModel = this._intermediateCompilation.Compilation.GetSemanticModel( reference.RootExpression.SyntaxTree );

                    return this._inlinerProvider.TryGetInliner( reference, semanticModel, out inliner );
                }
            }

            /// <summary>
            /// Gets all semantics that are inlined.
            /// </summary>
            /// <param name="inlineableSemantics"></param>
            /// <param name="inlineableReferences"></param>
            /// <returns></returns>
            public IReadOnlyList<IntermediateSymbolSemantic> GetInlinedSemantics(
                IReadOnlyList<IntermediateSymbolSemantic> inlineableSemantics,
                IReadOnlyDictionary<ResolvedAspectReference, Inliner> inlineableReferences )
            {
                var inlineableSemanticHashSet = new HashSet<IntermediateSymbolSemantic>( inlineableSemantics );
                var inlinedSemantics = new List<IntermediateSymbolSemantic>();

                foreach ( var inlineableSemantic in inlineableSemantics )
                {
                    if ( IsInlinedSemantic( inlineableSemantic ) )
                    {
                        inlinedSemantics.Add( inlineableSemantic );
                    }
                }

                return inlinedSemantics;

                bool IsInlinedSemanticBody( IntermediateSymbolSemantic<IMethodSymbol> semanticBody )
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

                bool IsInlinedSemantic( IntermediateSymbolSemantic semantic )
                {
                    switch ( semantic.Symbol )
                    {
                        case IMethodSymbol
                        {
                            MethodKind: MethodKind.Ordinary or MethodKind.ExplicitInterfaceImplementation or MethodKind.UserDefinedOperator
                            or MethodKind.Conversion or MethodKind.Destructor
                        }:
                            return IsInlinedSemanticBody( semantic.ToTyped<IMethodSymbol>() );

                        case IPropertySymbol property:
                            // Property is inlined if at least one of the accessors is reachable and not inlineable.
                            var hasNonInlinedGet =
                                property.GetMethod != null
                                && !IsInlinedSemanticBody( semantic.WithSymbol( property.GetMethod ) )
                                && this._reachableSymbolSemantics.Contains( semantic.WithSymbol( property.GetMethod ) );

                            var hasNonInlinedSet =
                                property.SetMethod != null
                                && !IsInlinedSemanticBody( semantic.WithSymbol( property.SetMethod ) )
                                && this._reachableSymbolSemantics.Contains( semantic.WithSymbol( property.SetMethod ) );

                            return inlineableSemanticHashSet.Contains( semantic ) && !hasNonInlinedGet && !hasNonInlinedSet;

                        case IEventSymbol @event:
                            // Event is inlined if at least one of the accessors is reachable and not inlineable.
                            var hasNonInlinedAdd =
                                !IsInlinedSemanticBody( semantic.WithSymbol( @event.AddMethod.AssertNotNull() ) )
                                && this._reachableSymbolSemantics.Contains( semantic.WithSymbol( @event.AddMethod.AssertNotNull() ) );

                            var hasNonInlinedRemove =
                                !IsInlinedSemanticBody( semantic.WithSymbol( @event.RemoveMethod.AssertNotNull() ) )
                                && this._reachableSymbolSemantics.Contains( semantic.WithSymbol( @event.RemoveMethod.AssertNotNull() ) );

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
            public IReadOnlyDictionary<ResolvedAspectReference, Inliner> GetInlinedReferences(
                IReadOnlyDictionary<ResolvedAspectReference, Inliner> inlineableReferences,
                IReadOnlyList<IntermediateSymbolSemantic> inlinedSemantics )
            {
                var inlinedReferences = new Dictionary<ResolvedAspectReference, Inliner>();

                foreach ( var inlinedSemantic in inlinedSemantics )
                {
                    foreach ( var reference in this.GetReachableReferencesByTarget( inlinedSemantic.ToAspectReferenceTarget() ) )
                    {
                        if ( !inlineableReferences.TryGetValue( reference, out var inliner ) )
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