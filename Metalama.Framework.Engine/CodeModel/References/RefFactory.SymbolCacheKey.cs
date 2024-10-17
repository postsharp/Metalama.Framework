// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel.GenericContexts;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.References;

internal sealed partial class RefFactory
{
    private readonly record struct SymbolCacheKey( ISymbol Symbol, RefTargetKind TargetKind, GenericContext GenericContext );
}