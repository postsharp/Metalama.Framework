// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class IndexerList : MemberOrNamedTypeList<IIndexer, MemberRef<IIndexer>>, IIndexerList
{
    public IndexerList( NamedType containingDeclaration, IEnumerable<MemberRef<IIndexer>> sourceItems ) : base( containingDeclaration, sourceItems ) { }
}