// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.References;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class NamedTypeList : MemberList<INamedType, MemberRef<INamedType>>, INamedTypeList
    {
        public NamedTypeList( INamedType containingDeclaration, IEnumerable<MemberRef<INamedType>> sourceItems ) : base( containingDeclaration, sourceItems ) { }

        public NamedTypeList( ICompilation containingDeclaration, IEnumerable<MemberRef<INamedType>> sourceItems ) : base( containingDeclaration, sourceItems ) { }
    }
}