// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal abstract class PropertyInliner : Inliner 
    {
        public override bool IsValidForTargetSymbol( ISymbol symbol )
        {
            return
                symbol is IPropertySymbol
                || symbol is IMethodSymbol { AssociatedSymbol: IPropertySymbol };
        }

        public override bool IsValidForContainingSymbol( ISymbol symbol )
        {
            return true;
        }
    }
}