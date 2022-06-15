// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CodeModel.Builders;
using Metalama.Framework.Engine.Collections;
using Metalama.Framework.Engine.Transformations;
using Metalama.Framework.Engine.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MethodKind = Microsoft.CodeAnalysis.MethodKind;
using TypeKind = Microsoft.CodeAnalysis.TypeKind;

// Ordered declaration versions (intermediate compilation):
//  * Overridden declaration (base class declaration)
//  * Target declaration, base semantic (if from source code)
//  * Target declaration, default semantic (if introduced, no overridden declaration)
//  * Override 1-1
//  ...
//  * Override z-1
//  ...
//  * Override z-k (there are multiple overrides of the same declaration on one layer).
//  ...
//  * Override n
//  * Target declaration,final semantic)

// Each of above correspond to an aspect layer in the global order.
// The reference we are trying to resolve also originates in of the aspect layers.

// Declaration semantics projected to global aspect layer order:
// * Layer (0, 0):   Overridden declaration (base class declaration).
// * Layer (0, 0):   Target declaration, default semantic (if from source code).
// * Layer (0, 0):   Target declaration, base semantic (if introduced, no overridden declaration).
// ...
// * Layer (k, 0):   Target declaration, default semantic (if introduced).
// * Layer (k, 1):   After override 1-1 (same layer as introduction).
// ...
// * Layer (l_1, 1): After override 2-1 (layer with multiple overrides).
// ...
// * Layer (l_1, k): After override 2-k.
// ...
// * Layer (l_n, 1): After override n.
// ...
// * Layer (m, 0):   Target declaration, final semantic.

// AspectReferenceOrder resolution:
//  * Original - resolves to the first in the order.
//  * Base - resolved to the last override preceding the origin layer.
//  * Self - resolved to the last override preceding or equal to the origin layer.
//  * Final - resolved to the last in the order.

// Special cases:
//  * Promoted fields do not count as introductions. The layer of the promotion target applies.
//    Source promoted fields are treated as source declarations. Introduced and then promoted fields
//    are treated as being introduced at the point of field introduction.

// Notes:
//  * Base and Self are different only for layers that override the referenced declaration.

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Resolves aspect references.
    /// </summary>
    internal class AspectReferenceResolver
    {
        private readonly LinkerIntroductionRegistry _introductionRegistry;
        private readonly IReadOnlyList<AspectLayerId> _orderedLayers;
        private readonly IReadOnlyDictionary<AspectLayerId, int> _layerIndex;
        private readonly CompilationModel _finalCompilationModel;
        private readonly Compilation _intermediateCompilation;

        public AspectReferenceResolver(
            LinkerIntroductionRegistry introductionRegistry,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers,
            CompilationModel finalCompilationModel,
            Compilation intermediateCompilation )
        {
            this._introductionRegistry = introductionRegistry;

            var indexedLayers =
                new[] { AspectLayerId.Null }
                    .Concat( orderedAspectLayers.Select( x => x.AspectLayerId ) )
                    .Select( ( al, i ) => (AspectLayerId: al, Index: i) )
                    .ToList();

            this._orderedLayers = indexedLayers.Select( x => x.AspectLayerId ).ToReadOnlyList();
            this._layerIndex = indexedLayers.ToDictionary( x => x.AspectLayerId, x => x.Index );
            this._finalCompilationModel = finalCompilationModel;
            this._intermediateCompilation = intermediateCompilation;
        }

        public ResolvedAspectReference Resolve(
            ISymbol containingSymbol,
            ISymbol referencedSymbol,
            ExpressionSyntax expression,
            AspectReferenceSpecification referenceSpecification )
        {
            // Get the local symbol that is referenced.
            // E.g. explicit interface implementation must be referenced as interface member reference.
            referencedSymbol = GetLocalReferencedSymbol( containingSymbol, referencedSymbol );

            var annotationLayerIndex = this.GetAnnotationLayerIndex( containingSymbol, referencedSymbol, referenceSpecification );

            // If the override target was introduced, determine the index.
            var targetMemberIntroduction = this._introductionRegistry.GetIntroducedMemberForSymbol( referencedSymbol );
            var targetMemberIntroductionIndex = this.GetIntroductionLogicalIndex( targetMemberIntroduction );

            var overrideIndices = this.GetOverrideIndices( referencedSymbol );

            this.ResolveLayerIndex(
                referenceSpecification,
                annotationLayerIndex,
                targetMemberIntroduction,
                targetMemberIntroductionIndex,
                overrideIndices,
                out var resolvedIndex,
                out var resolvedIntroducedMember );

            if ( referencedSymbol is IFieldSymbol field )
            {
                // Field symbols are resolved to themselves (this may be temporary).
                var fieldSemantic =
                    targetMemberIntroduction == null
                        ? IntermediateSymbolSemanticKind.Default
                        : resolvedIndex < targetMemberIntroductionIndex
                            ? IntermediateSymbolSemanticKind.Base
                            : IntermediateSymbolSemanticKind.Default;

                return new ResolvedAspectReference(
                    containingSymbol,
                    referencedSymbol,
                    new IntermediateSymbolSemantic<IFieldSymbol>( field, fieldSemantic ),
                    expression,
                    referenceSpecification );
            }

            // At this point resolvedIndex should be 0, equal to target introduction index, this._orderedLayers.Count or be equal to index of one of the overrides.
            Invariant.Assert(
                resolvedIndex == default
                || resolvedIndex == new MemberLayerIndex( this._orderedLayers.Count, 0 )
                || overrideIndices.Any( x => x.Index == resolvedIndex )
                || resolvedIndex == targetMemberIntroductionIndex );

            if ( overrideIndices.Count > 0 && resolvedIndex == overrideIndices[overrideIndices.Count - 1].Index )
            {
                // If we have resolved to the last override, transition to the final declaration index.
                resolvedIndex = new MemberLayerIndex( this._orderedLayers.Count, 0 );
            }

            if ( resolvedIndex == default )
            {
                if ( targetMemberIntroduction == null )
                {
                    // There is no introduction, i.e. this is a user source symbol.
                    return new ResolvedAspectReference(
                        containingSymbol,
                        referencedSymbol,
                        new IntermediateSymbolSemantic(
                            referencedSymbol,
                            IntermediateSymbolSemanticKind.Default ),
                        expression,
                        referenceSpecification );
                }
                else
                {
                    // There is an introduction and this reference points to a state before that introduction.
                    if ( referencedSymbol.IsOverride )
                    {
                        // Introduction is an override, resolve to symbol in the base class.
                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            new IntermediateSymbolSemantic(
                                GetOverriddenSymbol( referencedSymbol ).AssertNotNull(),
                                IntermediateSymbolSemanticKind.Default ),
                            expression,
                            referenceSpecification );
                    }
                    else if ( targetMemberIntroduction.Introduction is IReplaceMemberTransformation { ReplacedMember: { } replacedMember }
                              && replacedMember.GetTarget( this._finalCompilationModel, false ).GetSymbol() != null )
                    {
                        // Introduction replaced existing source member, resolve to default semantics, i.e. source symbol.

                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            new IntermediateSymbolSemantic( referencedSymbol, IntermediateSymbolSemanticKind.Default ),
                            expression,
                            referenceSpecification );
                    }
                    else
                    {
                        // Introduction is a new member, resolve to base semantics, i.e. empty method.
                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            new IntermediateSymbolSemantic(
                                referencedSymbol,
                                IntermediateSymbolSemanticKind.Base ),
                            expression,
                            referenceSpecification );
                    }
                }
            }
            else if ( resolvedIndex == targetMemberIntroductionIndex )
            {
                // We have resolved to the target member introduction.
                // The only way to get here is using "Base" order in the first override.
                if ( HasImplicitImplementation( referencedSymbol ) )
                {
                    return new ResolvedAspectReference(
                        containingSymbol,
                        referencedSymbol,
                        new IntermediateSymbolSemantic(
                            referencedSymbol,
                            IntermediateSymbolSemanticKind.Default ),
                        expression,
                        referenceSpecification );
                }
                else
                {
                    if ( referencedSymbol.IsOverride )
                    {
                        // Introduction is an override, resolve to symbol in the base class.
                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            new IntermediateSymbolSemantic(
                                GetOverriddenSymbol( referencedSymbol ).AssertNotNull(),
                                IntermediateSymbolSemanticKind.Default ),
                            expression,
                            referenceSpecification );
                    }
                    else if ( this.TryGetHiddenSymbol( referencedSymbol, out var hiddenSymbol ) )
                    {
                        // The introduction is hiding another member, resolve to default semantics.
                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            new IntermediateSymbolSemantic(
                                hiddenSymbol,
                                IntermediateSymbolSemanticKind.Default ),
                            expression,
                            referenceSpecification );
                    }
                    else
                    {
                        // Introduction is a new member, resolve to base semantics, i.e. empty method.
                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            new IntermediateSymbolSemantic(
                                referencedSymbol,
                                IntermediateSymbolSemanticKind.Base ),
                            expression,
                            referenceSpecification );
                    }
                }
            }
            else if ( resolvedIndex.LayerIndex < this._orderedLayers.Count )
            {
                // One of the overrides or the introduced member.
                if ( targetMemberIntroduction != null && resolvedIndex.MemberIndex == 0 )
                {
                    // TODO: This would happen has the introduced member contained aspect reference. Bodies of introduced members are
                    //       currently not used.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                    // // There is no introduction, i.e. this is a user source symbol.
                    // return new ResolvedAspectReference(
                    //     containingSymbol,
                    //     referencedSymbol,
                    //     new IntermediateSymbolSemantic(
                    //         this.GetSymbolFromIntroducedMember( referencedSymbol, targetMemberIntroduction.AssertNotNull() ),
                    //         IntermediateSymbolSemanticKind.Default ),
                    //     expression,
                    //     referenceSpecification );
                }
                else
                {
                    return new ResolvedAspectReference(
                        containingSymbol,
                        referencedSymbol,
                        new IntermediateSymbolSemantic(
                            this.GetSymbolFromIntroducedMember( referencedSymbol, resolvedIntroducedMember.AssertNotNull() ),
                            IntermediateSymbolSemanticKind.Default ),
                        expression,
                        referenceSpecification );
                }
            }
            else
            {
                return new ResolvedAspectReference(
                    containingSymbol,
                    referencedSymbol,
                    new IntermediateSymbolSemantic(
                        referencedSymbol,
                        IntermediateSymbolSemanticKind.Final ),
                    expression,
                    referenceSpecification );
            }
        }

        private void ResolveLayerIndex(
            AspectReferenceSpecification referenceSpecification,
            MemberLayerIndex annotationLayerIndex,
            LinkerIntroducedMember? targetMemberIntroduction,
            MemberLayerIndex? targetMemberIntroductionIndex,
            IReadOnlyList<(MemberLayerIndex Index, LinkerIntroducedMember Override)> overrideIndices,
            out MemberLayerIndex resolvedIndex,
            out LinkerIntroducedMember? resolvedIntroducedMember )
        {
            resolvedIntroducedMember = null;

            switch ( referenceSpecification.Order )
            {
                case AspectReferenceOrder.Original:
                    resolvedIndex = default;

                    break;

                case AspectReferenceOrder.Base:
                    // TODO: optimize.

                    var lowerOverride = overrideIndices.LastOrDefault( x => x.Index < annotationLayerIndex );

                    if ( lowerOverride.Override != null )
                    {
                        resolvedIndex = lowerOverride.Index;
                        resolvedIntroducedMember = lowerOverride.Override;
                    }
                    else if ( targetMemberIntroductionIndex != null && targetMemberIntroductionIndex.Value < annotationLayerIndex )
                    {
                        resolvedIndex = targetMemberIntroductionIndex.Value;
                        resolvedIntroducedMember = targetMemberIntroduction;
                    }
                    else
                    {
                        resolvedIndex = default;
                    }

                    break;

                case AspectReferenceOrder.Self:
                    // TODO: optimize.

                    var lowerOrEqualOverride = overrideIndices.LastOrDefault( x => x.Index <= annotationLayerIndex );

                    if ( lowerOrEqualOverride.Override != null )
                    {
                        resolvedIndex = lowerOrEqualOverride.Index;
                        resolvedIntroducedMember = lowerOrEqualOverride.Override;
                    }
                    else if ( targetMemberIntroductionIndex != null && targetMemberIntroductionIndex.Value <= annotationLayerIndex )
                    {
                        resolvedIndex = targetMemberIntroductionIndex.Value;
                        resolvedIntroducedMember = targetMemberIntroduction;
                    }
                    else
                    {
                        resolvedIndex = default;
                    }

                    break;

                case AspectReferenceOrder.Final:
                    resolvedIndex = new MemberLayerIndex( this._orderedLayers.Count, 0 );

                    break;

                default:
                    throw new AssertionFailedException();
            }
        }

        private IReadOnlyList<(MemberLayerIndex Index, LinkerIntroducedMember Override)> GetOverrideIndices( ISymbol referencedSymbol )
        {
            var referencedDeclarationOverrides = this._introductionRegistry.GetOverridesForSymbol( referencedSymbol );

            // Compute indices of overrides of the referenced declaration.
            return (from overrideIntroduction in referencedDeclarationOverrides
                    group overrideIntroduction by overrideIntroduction.AspectLayerId
                    into g
                    select g.Select( ( o, i ) => (Index: new MemberLayerIndex( this._layerIndex[o.AspectLayerId], i + 1 ), Override: o) )
                ).SelectMany( g => g )
                .ToReadOnlyList();
        }

        private MemberLayerIndex? GetIntroductionLogicalIndex( LinkerIntroducedMember? introducedMember )
        {
            // This supports only field promotions.
            if ( introducedMember == null )
            {
                return null;
            }

            if ( introducedMember.Introduction is IReplaceMemberTransformation { ReplacedMember: { } replacedMemberRef } )
            {
                var replacedMember = replacedMemberRef.GetTarget( this._finalCompilationModel, false );

                IDeclaration canonicalReplacedMember = replacedMember switch
                {
                    BuiltDeclaration builtDeclaration => builtDeclaration.Builder,
                    _ => replacedMember
                };

                if ( canonicalReplacedMember is ITransformation replacedTransformation )
                {
                    // This is introduced field, which is then promoted. Semantics of the field and of the property are the same.
                    return new MemberLayerIndex( this._layerIndex[replacedTransformation.Advice.AspectLayerId], 0 );
                }
                else
                {
                    // This is promoted source declaration we treat it as being present from the beginning.
                    return new MemberLayerIndex( 0, 0 );
                }
            }

            return new MemberLayerIndex( this._layerIndex[introducedMember.AspectLayerId], 0 );
        }

        private MemberLayerIndex GetAnnotationLayerIndex(
            ISymbol containingSymbol,
            ISymbol referencedSymbol,
            AspectReferenceSpecification referenceSpecification )
        {
            var referencedDeclarationOverrides = this._introductionRegistry.GetOverridesForSymbol( referencedSymbol );

            var containedInTargetOverride =
                this._introductionRegistry.IsOverrideTarget( referencedSymbol )
                && referencedDeclarationOverrides.Any(
                    x => SymbolEqualityComparer.Default.Equals(
                        this._introductionRegistry.GetSymbolForIntroducedMember( x ),
                        GetPrimarySymbol( containingSymbol ) ) );

            // TODO: Optimize (most of this can be precomputed).
            // TODO: Support multiple overrides in the same layer (the memberIndex has to be determined).
            // Determine the layer from which this reference originates.
            //  * If the reference is coming from and override of the same declaration it's referencing, we need to select the correct override index.
            //  * Otherwise, treat the reference as coming from the last override of the declaration.
            var annotationLayerIndex =
                containedInTargetOverride
                    ? new MemberLayerIndex(
                        this._layerIndex[referenceSpecification.AspectLayerId],
                        referencedDeclarationOverrides
                            .Where( x => x.AspectLayerId == referenceSpecification.AspectLayerId )
                            .Select( ( x, i ) => (Symbol: x, Index: i + 1) )
                            .Single(
                                x =>
                                    SymbolEqualityComparer.Default.Equals(
                                        this._introductionRegistry.GetSymbolForIntroducedMember( x.Symbol ),
                                        GetPrimarySymbol( containingSymbol ) ) )
                            .Index )
                    : new MemberLayerIndex(
                        this._layerIndex[referenceSpecification.AspectLayerId],
                        referencedDeclarationOverrides.Count( x => x.AspectLayerId == referenceSpecification.AspectLayerId ) );

            return annotationLayerIndex;
        }

        private static ISymbol GetLocalReferencedSymbol( ISymbol containingSymbol, ISymbol referencedSymbol )
        {
            // Check whether we are referencing explicit interface implementation.
            if ( (!SymbolEqualityComparer.Default.Equals( containingSymbol.ContainingType, referencedSymbol.ContainingType )
                  && referencedSymbol.ContainingType.TypeKind == TypeKind.Interface)
                 || referencedSymbol.IsInterfaceMemberImplementation() )
            {
                // TODO: For some reason we get here in two ways (see the condition):
                //          1) The symbol is directly interface symbol (first condition matches).
                //          2) sometimes it is a "reference", i.e. contained within the current type (second condition matches).
                //       This may depend on the declaring assembly or on presence of compilation errors.

                // It's not possible to reference the introduced explicit interface implementation directly, so the reference expression
                // is in form "((<interface_type>)this).<member>", which means that the symbol points to interface member. We will transition
                // to the real member (explicit implementation) of the type before doing the rest of resolution.

                // Replace the referenced symbol with the overridden interface implementation.                
                return containingSymbol.ContainingType.AssertNotNull().FindImplementationForInterfaceMember( referencedSymbol ).AssertNotNull();
            }

            if ( referencedSymbol is IMethodSymbol { Name: "Finalizer", ContainingType: { Name: "__LinkerIntroductionHelpers__" } } )
            {
                // Referencing type's finalizer.
                return containingSymbol.ContainingType.GetMembers( "Finalize" ).OfType<IMethodSymbol>().Single( m => m.Parameters.Length == 0 && m.TypeParameters.Length == 0 );
            }

            return referencedSymbol;
        }

        private static bool HasImplicitImplementation( ISymbol symbol )
        {
            switch ( symbol )
            {
                case IPropertySymbol property when property.IsAutoProperty():
                case IEventSymbol @event when @event.IsExplicitInterfaceEventField() || @event.IsEventField():
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Translates the resolved introduction to the same kind of symbol as the referenced symbol.
        /// </summary>
        /// <param name="referencedSymbol"></param>
        /// <param name="resolvedIntroduction"></param>
        /// <returns></returns>
        private ISymbol GetSymbolFromIntroducedMember( ISymbol referencedSymbol, LinkerIntroducedMember resolvedIntroduction )
        {
            var symbol = this._introductionRegistry.GetSymbolForIntroducedMember( resolvedIntroduction );

            return GetCorrespondingSymbolForResolvedSymbol( referencedSymbol, symbol );
        }

        private static ISymbol? GetOverriddenSymbol( ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol methodSymbol => methodSymbol.OverriddenMethod,
                IPropertySymbol propertySymbol => propertySymbol.OverriddenProperty,
                IEventSymbol eventSymbol => eventSymbol.OverriddenEvent,
                _ => throw new AssertionFailedException()
            };

        private static ISymbol? GetPrimarySymbol( ISymbol symbol )
            => symbol switch
            {
                IMethodSymbol { MethodKind: MethodKind.PropertyGet or MethodKind.PropertySet } methodSymbol => methodSymbol.AssociatedSymbol,
                IMethodSymbol { MethodKind: MethodKind.EventAdd or MethodKind.EventRemove or MethodKind.EventRaise } methodSymbol => methodSymbol
                    .AssociatedSymbol,
                IMethodSymbol methodSymbol => methodSymbol,
                IPropertySymbol propertySymbol => propertySymbol,
                IEventSymbol eventSymbol => eventSymbol,
                _ => throw new AssertionFailedException()
            };

        /// <summary>
        /// Gets a symbol the "new" symbol is hiding.
        /// </summary>
        /// <param name="symbol">The hiding symbol.</param>
        /// <param name="hiddenSymbol">The hidden symbol.</param>
        /// <returns>Hidden symbol or null.</returns>
        private bool TryGetHiddenSymbol( ISymbol symbol, [NotNullWhen( true )] out ISymbol? hiddenSymbol )
        {
            var currentType = symbol.ContainingType.BaseType;

            while ( currentType != null )
            {
                var matchingSymbol = currentType.GetMembers()
                    .SingleOrDefault(
                        member => member.IsVisibleTo( this._intermediateCompilation, symbol ) && StructuralSymbolComparer.Signature.Equals( symbol, member ) );

                if ( matchingSymbol != null )
                {
                    hiddenSymbol = matchingSymbol;

                    return true;
                }

                currentType = currentType.BaseType;
            }

            hiddenSymbol = null;

            return false;
        }

        /// <summary>
        /// Gets a symbol that corresponds to the referenced symbol for the resolved symbol. 
        /// This has a meaning when referenced symbol was a property/event accessor and the resolved symbol is the property/event itself.
        /// </summary>
        /// <param name="referencedSymbol"></param>
        /// <param name="resolvedSymbol"></param>
        /// <returns></returns>
        private static ISymbol GetCorrespondingSymbolForResolvedSymbol( ISymbol referencedSymbol, ISymbol resolvedSymbol )
        {
            switch (referencedSymbol, resolvedSymbol)
            {
                case (IMethodSymbol { MethodKind: MethodKind.Ordinary }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
                case (IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
                case (IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation },
                    IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation }):
                case (IMethodSymbol { MethodKind: MethodKind.Destructor }, IMethodSymbol { MethodKind: MethodKind.Ordinary } ):
                case (IPropertySymbol, IPropertySymbol):
                case (IEventSymbol, IEventSymbol):
                case (IFieldSymbol, IFieldSymbol):
                    return resolvedSymbol;

                case (IMethodSymbol { MethodKind: MethodKind.PropertyGet }, IPropertySymbol):
                    // This seems to happen only in invalid compilations.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                // return propertySymbol.GetMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.PropertySet }, IPropertySymbol):
                    // This seems to happen only in invalid compilations.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                // return propertySymbol.SetMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.EventAdd }, IEventSymbol):
                    // This seems to happen only in invalid compilations.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                // return eventSymbol.AddMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.EventRemove }, IEventSymbol):
                    // This seems to happen only in invalid compilations.
                    throw new AssertionFailedException( Justifications.CoverageMissing );

                // return eventSymbol.RemoveMethod.AssertNotNull();

                default:
                    throw new AssertionFailedException();
            }
        }

        private readonly struct MemberLayerIndex : IComparable<MemberLayerIndex>, IEquatable<MemberLayerIndex>
        {
            public int LayerIndex { get; }

            public int MemberIndex { get; }

            public MemberLayerIndex( int layerIndex, int memberIndex )
            {
                this.LayerIndex = layerIndex;
                this.MemberIndex = memberIndex;
            }

            public int CompareTo( MemberLayerIndex other )
            {
                var layerDiff = this.LayerIndex - other.LayerIndex;

                if ( layerDiff == 0 )
                {
                    return this.MemberIndex - other.MemberIndex;
                }
                else
                {
                    return layerDiff;
                }
            }

            public bool Equals( MemberLayerIndex other )
            {
                return this.CompareTo( other ) == 0;
            }

            public override bool Equals( object obj )
            {
                return obj is MemberLayerIndex mli && this.Equals( mli );
            }

            public override int GetHashCode()
            {
                return HashCode.Combine( this.LayerIndex, this.MemberIndex );
            }

            public override string ToString()
            {
                return $"({this.LayerIndex}, {this.MemberIndex})";
            }

            public static bool operator ==( MemberLayerIndex a, MemberLayerIndex b ) => a.Equals( b );

            public static bool operator !=( MemberLayerIndex a, MemberLayerIndex b ) => !a.Equals( b );

            public static bool operator <( MemberLayerIndex a, MemberLayerIndex b ) => a.CompareTo( b ) < 0;

            public static bool operator <=( MemberLayerIndex a, MemberLayerIndex b ) => a.CompareTo( b ) <= 0;

            public static bool operator >( MemberLayerIndex a, MemberLayerIndex b ) => a.CompareTo( b ) > 0;

            public static bool operator >=( MemberLayerIndex a, MemberLayerIndex b ) => a.CompareTo( b ) >= 0;
        }
    }
}