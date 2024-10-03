// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Abstractions;

internal interface ISymbolBasedCompilationElement : ICompilationElementImpl
{
    ISymbol Symbol { get; }
}

internal interface ICustomTranslatableDeclaration : IDeclarationImpl { }