// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;
using System;
using System.Linq;

namespace Metalama.Framework.Engine.Utilities;

public static class SourceGeneratorHelper
{
    public static bool IsGeneratedFile( SyntaxTree syntaxTree )
        => syntaxTree.FilePath.StartsWith( "Metalama.Framework.CompilerExtensions", StringComparison.Ordinal );

    public static bool IsGeneratedSymbol( ISymbol symbol )
    {
        if ( symbol.DeclaringSyntaxReferences.IsEmpty )
        {
            if ( symbol is IMethodSymbol { AssociatedSymbol: { } associatedSymbol } )
            {
                return IsGeneratedSymbol( associatedSymbol );
            }

            if ( symbol.ContainingSymbol != null )
            {
                return IsGeneratedSymbol( symbol.ContainingSymbol );
            }
            else
            {
                return false;
            }
        }
        else
        {
            return symbol.DeclaringSyntaxReferences.All( r => IsGeneratedFile( r.SyntaxTree ) );
        }
    }
}