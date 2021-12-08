// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Impl.CodeModel.References;
using System;
using System.Collections.Generic;

namespace Metalama.Framework.Impl.CodeModel.Collections
{
    internal class NamedTypeList : MemberOrNamedTypeList<INamedType, MemberRef<INamedType>>, INamedTypeList
    {
        public NamedTypeList( INamedType containingDeclaration, IEnumerable<MemberRef<INamedType>> sourceItems ) :
            base( containingDeclaration, sourceItems ) { }

        public NamedTypeList( ICompilation containingDeclaration, IEnumerable<MemberRef<INamedType>> sourceItems ) :
            base( containingDeclaration, sourceItems ) { }

        public NamedTypeList( INamespace containingDeclaration, IEnumerable<MemberRef<INamedType>> sourceItems ) :
            base( containingDeclaration, sourceItems ) { }

        IReadOnlyList<INamedType> INamedTypeList.DerivedFrom( Type type ) => throw new NotImplementedException();

        IReadOnlyList<INamedType> INamedTypeList.DerivedFrom( INamedType type ) => throw new NotImplementedException();
    }
}