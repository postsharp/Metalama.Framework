// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Source;

internal interface ISymbolBasedCompilationElement : ICompilationElement
{
    ISymbol Symbol { get; }

    IGenericContext? GenericContextForSymbolMapping { get; }
}