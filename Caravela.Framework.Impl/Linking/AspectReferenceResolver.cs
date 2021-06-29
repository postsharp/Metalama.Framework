// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Utilities;
using Microsoft.CodeAnalysis;
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

        public ISymbol Resolve( ISymbol referencedSymbol, AspectReferenceSpecification aspectReference )
        {
            // TODO: Other things than methods.
            var overrides = this._introductionRegistry.GetOverridesForSymbol( referencedSymbol );
            var indexedLayers = this._orderedAspectLayers.Select( ( o, i ) => (AspectLayerId: o.AspectLayerId, Index: i) ).ToReadOnlyList();
            var annotationLayerIndex = indexedLayers.Single( x => x.AspectLayerId == aspectReference.AspectLayerId ).Index;

            // TODO: Optimize.
            var previousLayerOverride = (
                from o in overrides
                join oal in indexedLayers
                    on o.AspectLayerId equals oal.AspectLayerId
                where oal.Index < annotationLayerIndex
                orderby oal.Index
                select o
            ).LastOrDefault();

            if ( previousLayerOverride == null )
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

                return referencedSymbol;
            }

            return this._introductionRegistry.GetSymbolForIntroducedMember( previousLayerOverride );

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
                            hiddenSymbol = (IMethodSymbol) member;

                            return true;
                        }
                    }

                    currentType = currentType.BaseType;
                }

                hiddenSymbol = null;

                return false;
            }
        }
    }
}
