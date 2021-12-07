// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.CodeModel;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Impl.Linking.Inlining
{
    internal abstract class PropertyGetInliner : PropertyInliner
    {
        public override bool IsValidForTargetSymbol( ISymbol symbol )
        {
            var property =
                symbol is IPropertySymbol propertySymbol
                    ? propertySymbol
                    : symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol associatedProperty }
                        ? associatedProperty
                        : null;

            return property != null
                   && property.GetMethod != null
                   && !IteratorHelper.IsIterator( property.GetMethod );
        }
    }
}