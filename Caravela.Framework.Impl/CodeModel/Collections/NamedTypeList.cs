// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class NamedTypeList : MemberList<INamedType, MemberLink<INamedType>>, INamedTypeList
    {
        public NamedTypeList( INamedType containingElement, IEnumerable<MemberLink<INamedType>> sourceItems ) : base( containingElement, sourceItems )
        {
        }

        public NamedTypeList( ICompilation containingElement, IEnumerable<MemberLink<INamedType>> sourceItems ) : base( containingElement, sourceItems )
        {
        }
    }
}