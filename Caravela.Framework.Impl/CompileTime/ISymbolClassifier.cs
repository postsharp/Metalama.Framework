// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.CompileTime
{
    /// <summary>
    /// Determines the kind of symbol: template, <see cref="SymbolDeclarationScope.CompileTimeOnly"/>,
    /// <see cref="SymbolDeclarationScope.RunTimeOnly"/>.
    /// </summary>
    internal interface ISymbolClassifier
    {
        bool IsTemplate( ISymbol symbol );

        SymbolDeclarationScope GetSymbolDeclarationScope( ISymbol symbol );
    }
}