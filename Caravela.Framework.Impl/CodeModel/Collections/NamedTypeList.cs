// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class NamedTypeList : MemberList<INamedType, MemberLink<INamedType>>, INamedTypeList
    {
        public NamedTypeList( IEnumerable<MemberLink<INamedType>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }
    }
}