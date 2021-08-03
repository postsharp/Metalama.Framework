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
//  * Overridden declaration (introduced)
//  * Target declaration, base semantic (introduced, no overridden)
//  * Target declaration, default semantic (introduced or source code)
//  * Override 1
//  ...
//  * Override n
//  * Target declaration,final semantic)

// Each of above correspond to an aspect layer in the global order.
// The reference we are trying to resolve also originates in of the aspect layers.

// Declaration versions projected to global aspect layer order:
// * Layer 0:   Overridden declaration.
// * Layer 0:   Target declaration, default semantic (source code).
// * Layer 0:   Target declaration, base semantic (introduced, no overridden declaration).
// ...
// * Layer k:   Target Declaration, default semantic (introduced).
// ...
// * Layer l_1: Override 1.
// ...
// * Layer l_n: Override n.
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
        private readonly IReadOnlyList<OrderedAspectLayer> _orderedAspectLayers;

        public AspectReferenceResolver( LinkerIntroductionRegistry introductionRegistry, IReadOnlyList<OrderedAspectLayer> orderedAspectLayers )
        {
            this._introductionRegistry = introductionRegistry;
            this._orderedAspectLayers = orderedAspectLayers;
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
                // is in form "((<interface_type>)this).<member>", which means that the symbol points to interface member. We will transition to the real member of the type
                // before doing the rest of resolution.

                // Replace the referenced symbol with the overridden interface implementation.                
                referencedSymbol = containingSymbol.ContainingType.AssertNotNull().FindImplementationForInterfaceMember( referencedSymbol ).AssertNotNull();
            }

            // TODO: Optimize (most of this can be precomputed).
            var indexedLayers = this._orderedAspectLayers.Select( ( o, i ) => (o.AspectLayerId, Index: i) ).ToReadOnlyList();
            var annotationLayerIndex = indexedLayers.Single( x => x.AspectLayerId == referenceSpecification.AspectLayerId ).Index;

            var targetMemberIntroduction = this._introductionRegistry.GetIntroducedMemberForSymbol( referencedSymbol );
            var overrides = this._introductionRegistry.GetOverridesForSymbol( referencedSymbol );

            switch ( referenceSpecification.Order )
            {
                case AspectReferenceOrder.Original:
                    {
                        if ( targetMemberIntroduction != null )
                        {
                            // The target member is introduced.
                        }
                        else
                        {
                            var originalSymbol = GetSourceDeclarationSymbol( referencedSymbol );
                        }

                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            GetCorrespodingSymbolForResolvedSymbol( referencedSymbol, originalSymbol ),
                            SymbolEqualityComparer.Default.Equals( referencedSymbol, originalSymbol )
                                ? ResolvedAspectReferenceSemantic.Original
                                : ResolvedAspectReferenceSemantic.Default,
                            expression,
                            referenceSpecification );
                    }

                case AspectReferenceOrder.Self:
                    referencedIntroduction = GetClosestSucceedingOverride( overrides, indexedLayers, annotationLayerIndex );

                    if ( referencedIntroduction != null )
                    {
                        // There is a preceding override, resolve to override symbol.
                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            this.GetSymbolFromIntroducedMember( referencedSymbol, referencedIntroduction ),
                            ResolvedAspectReferenceSemantic.Default,
                            expression,
                            referenceSpecification );
                    }
                    else
                    {
                        // There is no preceding override, this is reference to the original body.
                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            GetCorrespodingSymbolForResolvedSymbol( referencedSymbol, referencedSymbol ),
                            ResolvedAspectReferenceSemantic.Default,
                            expression,
                            referenceSpecification );
                    }

                case AspectReferenceOrder.Base:
                    referencedIntroduction = GetClosestPrecedingOverride( overrides, indexedLayers, annotationLayerIndex );

                    if ( referencedIntroduction != null )
                    {
                        // There is a succeeding override, resolve to override symbol.
                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            this.GetSymbolFromIntroducedMember( referencedSymbol, referencedIntroduction ),
                            ResolvedAspectReferenceSemantic.Default,
                            expression,
                            referenceSpecification );
                    }
                    else
                    {
                        // No override after the referencing aspect, point to the final declaration.
                        var originalSymbol = GetSourceDeclarationSymbol( referencedSymbol );

                        return new ResolvedAspectReference(
                            containingSymbol,
                            referencedSymbol,
                            GetCorrespodingSymbolForResolvedSymbol( referencedSymbol, originalSymbol ),
                            SymbolEqualityComparer.Default.Equals( referencedSymbol, originalSymbol )
                                ? ResolvedAspectReferenceSemantic.Original
                                : ResolvedAspectReferenceSemantic.Default,
                            expression,
                            referenceSpecification );
                    }

                case AspectReferenceOrder.Final:
                    // Final reference order is always resolved to the final declaration.
                    return new ResolvedAspectReference(
                        containingSymbol,
                        referencedSymbol,
                        referencedSymbol,
                        ResolvedAspectReferenceSemantic.Default,
                        expression,
                        referenceSpecification );

                default:
                    throw new AssertionFailedException();
            }
            if ( overrides.Count > 0 )
            {
                LinkerIntroducedMember? referencedIntroduction;


            }
            else
            {
                return new ResolvedAspectReference(
                    containingSymbol,
                    referencedSymbol,
                    referencedSymbol,
                    ResolvedAspectReferenceSemantic.Default,
                    expression,
                    referenceSpecification );
            }
        }

        private static ISymbol GetSourceDeclarationSymbol( ISymbol referencedSymbol )
        {
            if ( referencedSymbol is IMethodSymbol methodSymbol )
            {
                if ( methodSymbol.OverriddenMethod != null )
                {
                    return methodSymbol.OverriddenMethod;
                }
            }
            else if ( referencedSymbol is IPropertySymbol propertySymbol )
            {
                if ( propertySymbol.OverriddenProperty != null )
                {
                    return propertySymbol.OverriddenProperty;
                }
            }
            else if ( referencedSymbol is IEventSymbol eventSymbol )
            {
                var overridenAccessor = eventSymbol.AddMethod?.OverriddenMethod ?? eventSymbol.RemoveMethod?.OverriddenMethod;

                if ( overridenAccessor != null )
                {
                    return overridenAccessor.AssociatedSymbol.AssertNotNull();
                }
            }

            return referencedSymbol;
        }

        private static LinkerIntroducedMember? GetClosestSucceedingOverride(
            IReadOnlyList<LinkerIntroducedMember>? overrides,
            IReadOnlyList<(AspectLayerId AspectLayerId, int Index)>? indexedLayers,
            int annotationLayerIndex )
        {
            return
            (
                from o in overrides
                join oal in indexedLayers
                    on o.AspectLayerId equals oal.AspectLayerId
                where oal.Index >= annotationLayerIndex
                orderby oal.Index
                select o
            ).FirstOrDefault();
        }

        private static LinkerIntroducedMember? GetClosestPrecedingOverride(
            IReadOnlyList<LinkerIntroducedMember>? overrides,
            IReadOnlyList<(AspectLayerId AspectLayerId, int Index)>? indexedLayers,
            int annotationLayerIndex )
        {
            return
            (
                from o in overrides
                join oal in indexedLayers
                    on o.AspectLayerId equals oal.AspectLayerId
                where oal.Index < annotationLayerIndex
                orderby oal.Index
                select o
            ).LastOrDefault();
        }

        private ISymbol GetSymbolFromIntroducedMember( ISymbol referencedSymbol, LinkerIntroducedMember resolvedIntroduction )
        {
            var symbol = this._introductionRegistry.GetSymbolForIntroducedMember( resolvedIntroduction );

            return GetCorrespodingSymbolForResolvedSymbol( referencedSymbol, symbol );
        }

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