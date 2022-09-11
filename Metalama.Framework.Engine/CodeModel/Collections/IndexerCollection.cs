// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class IndexerCollection : MemberCollection<IIndexer>, IIndexerCollection
{
    public IndexerCollection( NamedType declaringType, IndexerUpdatableCollection sourceItems ) : base( declaringType, sourceItems ) { }
}