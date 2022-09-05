// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Microsoft.CodeAnalysis;
using System;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class IndexerUpdatableCollection : UniquelyNamedTypeMemberUpdatableCollection<IIndexer>
{
    public IndexerUpdatableCollection( CompilationModel compilation, INamedTypeSymbol declaringType ) : base( compilation, declaringType ) { }

    protected override Func<ISymbol, bool> Predicate => p => p.Kind == SymbolKind.Property && ((IPropertySymbol) p).Parameters.Length > 0;
}