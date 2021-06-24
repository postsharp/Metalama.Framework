// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caravela.Framework.Impl.Linking
{
    public static class SymbolExtensions
    {
        public static SyntaxNode? GetPrimaryDeclaration(this ISymbol symbol)
        {
            // TODO: Partials.
            return symbol.DeclaringSyntaxReferences.Single().GetSyntax();
        }
    }
}
