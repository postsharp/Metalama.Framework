﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class IndexerUpdatableCollection : NonUniquelyNamedMemberUpdatableCollection<IIndexer>
{
    public IndexerUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override bool IsSymbolIncluded( ISymbol symbol )
        => symbol.Kind == SymbolKind.Property && ((IPropertySymbol) symbol).Parameters.Length > 0 && base.IsSymbolIncluded( symbol );
}