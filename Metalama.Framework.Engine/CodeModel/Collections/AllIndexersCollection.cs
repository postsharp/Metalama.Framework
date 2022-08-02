// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using System;

namespace Metalama.Framework.Engine.CodeModel.Collections;

internal class AllIndexersCollection : AllMembersCollection<IIndexer>, IIndexerCollection
{
    public AllIndexersCollection( NamedType declaringType ) : base( declaringType ) { }

    protected override IMemberCollection<IIndexer> GetMembers( INamedType namedType ) => namedType.Indexers;

    public IIndexer? OfExactSignature( IIndexer signatureTemplate, bool matchIsStatic = true ) => throw new NotImplementedException();
}