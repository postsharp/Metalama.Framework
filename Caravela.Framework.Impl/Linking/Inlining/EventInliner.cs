// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking.Inlining
{
    internal abstract class EventInliner : Inliner
    {
        public override bool IsValidForTargetSymbol( ISymbol symbol )
        {
            return
                symbol is IEventSymbol
                || symbol is IMethodSymbol { AssociatedSymbol: IEventSymbol };
        }

        public override bool IsValidForContainingSymbol( ISymbol symbol )
        {
            return true;
        }
    }
}