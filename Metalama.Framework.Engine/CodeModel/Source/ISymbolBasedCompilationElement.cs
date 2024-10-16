﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.Abstractions;
using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.Source;

internal interface ISymbolBasedCompilationElement : ICompilationElementImpl
{
    ISymbol Symbol { get; }

    GenericContext? GenericContextForSymbolMapping { get; }
}