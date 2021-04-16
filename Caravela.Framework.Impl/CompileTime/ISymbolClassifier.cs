// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    internal interface ISymbolClassifier
    {
        bool IsTemplate( ISymbol symbol );

        SymbolDeclarationScope GetSymbolDeclarationScope( ISymbol symbol );
    }
}
