// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.UpdatableCollections;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class NamedTypeCollection : MemberOrNamedTypeCollection<INamedType>, INamedTypeCollection
    {
        public NamedTypeCollection( NamedType declaringType, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringType, sourceItems ) { }

        public NamedTypeCollection( ICompilation declaringType, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringType, sourceItems ) { }

        public NamedTypeCollection( INamespace declaringType, UpdatableMemberCollection<INamedType> sourceItems ) :
            base( declaringType, sourceItems ) { }

        IReadOnlyList<INamedType> INamedTypeCollection.DerivedFrom( Type type ) => throw new NotImplementedException();

        IReadOnlyList<INamedType> INamedTypeCollection.DerivedFrom( INamedType type ) => throw new NotImplementedException();
    }
}