// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// Ordered declaration versions (intermediate compilation):
//  * Overridden declaration (base class declaration)
//  * Target declaration, base semantic (if from source code)
//  * Target declaration, default semantic (if introduced, no overridden declaration)
//  * Override 1
//  ...
//  * Override n
//  * Target declaration,final semantic)

// Each of above correspond to an aspect layer in the global order.
// The reference we are trying to resolve also originates in of the aspect layers.

// Declaration versions projected to global aspect layer order:
// * Layer 0:   Overridden declaration (base class declaration).
// * Layer 0:   Target declaration, default semantic (if from source code).
// * Layer 0:   Target declaration, base semantic (if introduced, no overridden declaration).
// ...
// * Layer k:   Target Declaration, default semantic (if introduced, overridden declaration exists).
// ...
// * Layer l_1: After override 1.
// ...
// * Layer l_n: After override n.
// ...
// * Layer m:   Target declaration, final semantic.

// AspectReferenceOrder resolution:
//  * Original - resolves to the first in the order.
//  * Base - resolved to the last override preceding the origin layer.
//  * Self - resolved to the last override preceding or equal to the origin layer.
//  * Final - resolved to the last in the order.

// Notes:
//  * Base and Self are different only for layers that override the referenced declaration.

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Resolves aspect references.
    /// </summary>
    internal class AspectReferenceResolver
    {
        private readonly LinkerIntroductionRegistry _introductionRegistry;
        private readonly IReadOnlyList<AspectLayerId> _orderedLayers;
        private readonly IReadOnlyDictionary<AspectLayerId, int> _layerIndex;

        public AspectReferenceResolver( LinkerIntroductionRegistry introductionRegistry, IReadOnlyList<OrderedAspectLayer> orderedAspectLayers )
        {
            this._introductionRegistry = introductionRegistry;
            var indexedLayers = 
                ((IEnumerable<AspectLayerId>)new[] { AspectLayerId.Null })
                .Concat( orderedAspectLayers.Select(x => x.AspectLayerId) )
                .Select( ( al, i ) => (AspectLayerId: al, Index: i) );

            this._orderedLayers = indexedLayers.Select( x => x.AspectLayerId ).ToReadOnlyList();
            this._layerIndex = indexedLayers.ToDictionary( x => x.AspectLayerId, x => x.Index );
        }

        public ResolvedAspectReference Resolve(
            ISymbol containingSymbol,
            ISymbol referencedSymbol,
            ExpressionSyntax expression,
            AspectReferenceSpecification referenceSpecification )
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
                referencedSymbol = containingSymbol.ContainingType.AssertNotNull().FindImplementationForInterfaceMember( referencedSymbol ).AssertNotNull();
            }

            // TODO: Optimize (most of this can be precomputed).
            var annotationLayerIndex = this._layerIndex[referenceSpecification.AspectLayerId];

            var targetMemberIntroduction = this._introductionRegistry.GetIntroducedMemberForSymbol( referencedSymbol );
            var targetMemberIntroductionIndex =
                targetMemberIntroduction != null
                ? this._layerIndex[targetMemberIntroduction.AspectLayerId]
                : default(int?);

            var overrideIntroductions = this._introductionRegistry.GetOverridesForSymbol( referencedSymbol );
            var overrideIndices = overrideIntroductions.Select( o => (Index: this._layerIndex[o.AspectLayerId], Override: o) ).OrderBy(x => x.Index).ToReadOnlyList();

            int resolvedIndex;
            LinkerIntroducedMember? resolvedIntroducedMember = null;

            switch ( referenceSpecification.Order )
            {
                case AspectReferenceOrder.Original:
                    resolvedIndex = 0;
                    break;

                case AspectReferenceOrder.Base:
                    // TODO: optimize.

                    var lowerOverride = overrideIndices.Where( x => x.Index < annotationLayerIndex ).LastOrDefault();

                    if (lowerOverride.Override != null)
                    {
                        resolvedIndex = lowerOverride.Index;
                        resolvedIntroducedMember = lowerOverride.Override;
                    }
                    else if ( targetMemberIntroductionIndex != null && targetMemberIntroductionIndex.Value < annotationLayerIndex)
                    {
                        resolvedIndex = targetMemberIntroductionIndex.Value;
                        resolvedIntroducedMember = targetMemberIntroduction;
                    }
                    else
                    {
                        resolvedIndex = 0;
                    }

                    break;

                case AspectReferenceOrder.Self:
                    // TODO: optimize.

                    var lowerOrEqualOverride = overrideIndices.Where( x => x.Index <= annotationLayerIndex ).LastOrDefault();

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
                        resolvedIndex = 0;
                    }

                    break;

                case AspectReferenceOrder.Final:
                    var lastOverride = overrideIndices.LastOrDefault();
                    resolvedIndex = this._orderedLayers.Count - 1;
                    break;

                default:
                    throw new AssertionFailedException();
            }

            // At this point resolvedIndex should be 0, this._orderedLayers.Count - 1 or be equal to index of one of the overrides.
            Invariant.Assert( resolvedIndex == 0 || resolvedIndex == this._orderedLayers.Count - 1 || overrideIndices.Any( x => x.Index == resolvedIndex ) );

            if ( resolvedIndex == 0)
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
                                GetOverriddenSymbol(referencedSymbol).AssertNotNull(),
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
            else if ( resolvedIndex < this._orderedLayers.Count - 1)
            {
                // One of the overrides or the introduced member.
                return new ResolvedAspectReference(
                    containingSymbol,
                    referencedSymbol,
                    new IntermediateSymbolSemantic(
                        this.GetSymbolFromIntroducedMember( referencedSymbol, resolvedIntroducedMember.AssertNotNull()),
                        IntermediateSymbolSemanticKind.Default ),
                    expression,
                    referenceSpecification );
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

        private ISymbol GetSymbolFromIntroducedMember( ISymbol referencedSymbol, LinkerIntroducedMember resolvedIntroduction )
        {
            var symbol = this._introductionRegistry.GetSymbolForIntroducedMember( resolvedIntroduction );

            return GetCorrespodingSymbolForResolvedSymbol( referencedSymbol, symbol );
        }

        private static ISymbol? GetOverriddenSymbol( ISymbol symbol ) => symbol switch
        {
            IMethodSymbol methodSymbol => methodSymbol.OverriddenMethod,
            IPropertySymbol propertySymbol => propertySymbol.OverriddenProperty,
            IEventSymbol eventSymbol => eventSymbol.OverriddenEvent,
            _ => throw new AssertionFailedException(),
        };

        /// <summary>
        /// Gets a symbol that corresponds to the referenced symbol for the resolved symbol. 
        /// This has a meaning when referenced symbol was a property/event accessor and the resolved symbol is the property/event itself.
        /// </summary>
        /// <param name="referencedSymbol"></param>
        /// <param name="resolvedSymbol"></param>
        /// <returns></returns>
        private static ISymbol GetCorrespodingSymbolForResolvedSymbol( ISymbol referencedSymbol, ISymbol resolvedSymbol )
        {
            switch (referencedSymbol, resolvedSymbol)
            {
                case (IMethodSymbol { MethodKind: MethodKind.Ordinary }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
                case (IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation }, IMethodSymbol { MethodKind: MethodKind.Ordinary }):
                case (IMethodSymbol { MethodKind: MethodKind.ExplicitInterfaceImplementation }, IMethodSymbol
                    { MethodKind: MethodKind.ExplicitInterfaceImplementation }):
                case (IPropertySymbol, IPropertySymbol):
                case (IEventSymbol, IEventSymbol):
                case (IFieldSymbol, IFieldSymbol):
                    return resolvedSymbol;

                case (IMethodSymbol { MethodKind: MethodKind.PropertyGet }, IPropertySymbol propertySymbol):
                    return propertySymbol.GetMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.PropertySet }, IPropertySymbol propertySymbol):
                    return propertySymbol.SetMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.EventAdd }, IEventSymbol eventSymbol):
                    return eventSymbol.AddMethod.AssertNotNull();

                case (IMethodSymbol { MethodKind: MethodKind.EventRemove }, IEventSymbol eventSymbol):
                    return eventSymbol.RemoveMethod.AssertNotNull();

                default:
                    throw new AssertionFailedException();
            }
        }
    }
}