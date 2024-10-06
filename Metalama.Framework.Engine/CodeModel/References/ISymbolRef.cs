// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

internal interface ISymbolRef : ICompilationBoundRefImpl
{
    ISymbol Symbol { get; }

    RefTargetKind TargetKind { get; }
}

internal interface ISymbolRef<out T> : ISymbolRef, IRefImpl<T> where T : class, ICompilationElement;