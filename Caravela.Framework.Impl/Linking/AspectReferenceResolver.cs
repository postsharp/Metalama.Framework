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

namespace Caravela.Framework.Impl.Linking
{
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
            // If the symbol containing the reference is different from the referenced symbol, we are referencing explicit interface implementation.
            if ( (!SymbolEqualityComparer.Default.Equals( containingSymbol.ContainingType, referencedSymbol.ContainingType )
                  && referencedSymbol.ContainingType.TypeKind == TypeKind.Interface)
                 || referencedSymbol.IsInterfaceMemberImplementation() )
            {
                // Replace the referenced symbol with the overridden interface implementation.
                referencedSymbol = containingSymbol.ContainingType.AssertNotNull().FindImplementationForInterfaceMember( referencedSymbol ).AssertNotNull();
            }

            var overrides = this._introductionRegistry.GetOverridesForSymbol( referencedSymbol );

            if ( overrides.Count > 0 )
            {
                var indexedLayers = this._orderedAspectLayers.Select( ( o, i ) => (o.AspectLayerId, Index: i) ).ToReadOnlyList();
                var annotationLayerIndex = indexedLayers.Single( x => x.AspectLayerId == referenceSpecification.AspectLayerId ).Index;

                // TODO: Optimize.
                LinkerIntroducedMember? referencedIntroduction;

                switch ( referenceSpecification.Order )
                {
                    case AspectReferenceOrder.Next:
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
                                referencedSymbol,
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
                            var originalSymbol = GetOriginalDeclarationSymbol( referencedSymbol );

                            return new ResolvedAspectReference(
                                containingSymbol,
                                referencedSymbol,
                                GetMethodSymbolForResolvedSymbol( referencedSymbol, originalSymbol ),
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

                    case AspectReferenceOrder.Original:
                        {
                            // Original reference order is always to the original declaration.
                            var originalSymbol = GetOriginalDeclarationSymbol( referencedSymbol );

                            return new ResolvedAspectReference(
                                containingSymbol,
                                referencedSymbol,
                                GetMethodSymbolForResolvedSymbol( referencedSymbol, originalSymbol ),
                                SymbolEqualityComparer.Default.Equals( referencedSymbol, originalSymbol )
                                    ? ResolvedAspectReferenceSemantic.Original
                                    : ResolvedAspectReferenceSemantic.Default,
                                expression,
                                referenceSpecification );
                        }

                    default:
                        throw new AssertionFailedException();
                }
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

            static bool TryGetHiddenSymbol( ISymbol symbol, [NotNullWhen( true )] out ISymbol? hiddenSymbol )
            {
                var currentType = symbol.ContainingType.BaseType;

                while ( currentType != null )
                {
                    // TODO: Optimize - lookup by name first instead of equating all members.
                    foreach ( var member in currentType.GetMembers() )
                    {
                        if ( StructuralSymbolComparer.Signature.Equals( symbol, member ) )
                        {
                            hiddenSymbol = member;

                            return true;
                        }
                    }

                    currentType = currentType.BaseType;
                }

                hiddenSymbol = null;

                return false;
            }

            static ISymbol GetOriginalDeclarationSymbol( ISymbol referencedSymbol )
            {
                if ( referencedSymbol is IMethodSymbol methodSymbol )
                {
                    if ( methodSymbol.OverriddenMethod != null )
                    {
                        return methodSymbol.OverriddenMethod;
                    }
                    else if ( TryGetHiddenSymbol( methodSymbol, out var hiddenSymbol ) )
                    {
                        return hiddenSymbol;
                    }
                }
                else if ( referencedSymbol is IPropertySymbol propertySymbol )
                {
                    var overridenAccessor = propertySymbol.GetMethod?.OverriddenMethod ?? propertySymbol.SetMethod?.OverriddenMethod;

                    if ( overridenAccessor != null )
                    {
                        return overridenAccessor.AssociatedSymbol.AssertNotNull();
                    }
                    else if ( TryGetHiddenSymbol( propertySymbol, out var hiddenSymbol ) )
                    {
                        return hiddenSymbol;
                    }
                }
                else if ( referencedSymbol is IEventSymbol eventSymbol )
                {
                    var overridenAccessor = eventSymbol.AddMethod?.OverriddenMethod ?? eventSymbol.RemoveMethod?.OverriddenMethod;

                    if ( overridenAccessor != null )
                    {
                        return overridenAccessor.AssociatedSymbol.AssertNotNull();
                    }
                    else if ( TryGetHiddenSymbol( eventSymbol, out var hiddenSymbol ) )
                    {
                        return hiddenSymbol;
                    }
                }

                return referencedSymbol;
            }
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
                where oal.Index > annotationLayerIndex
                orderby oal.Index
                select o
            ).LastOrDefault();
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

            return GetMethodSymbolForResolvedSymbol( referencedSymbol, symbol );
        }

        private static ISymbol GetMethodSymbolForResolvedSymbol( ISymbol referencedSymbol, ISymbol resolvedSymbol )
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