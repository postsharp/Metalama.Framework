// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.UpdatableCollections;

internal sealed class IndexerUpdatableCollection : NonUniquelyNamedUpdatableCollection<IIndexer>
{
    public IndexerUpdatableCollection( CompilationModel compilation, IRef<INamedType> declaringType ) : base(
        compilation,
        declaringType.As<INamespaceOrNamedType>() ) { }

    protected override IEqualityComparer<IRef<IIndexer>> MemberRefComparer => this.Compilation.CompilationContext.IndexerRefComparer;

    protected override DeclarationKind ItemsDeclarationKind => DeclarationKind.Indexer;
}