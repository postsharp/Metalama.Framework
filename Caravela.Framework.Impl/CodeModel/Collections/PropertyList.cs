// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class PropertyList : MemberList<IProperty, MemberLink<IProperty>>, IPropertyList
    {
        public PropertyList( IEnumerable<MemberLink<IProperty>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation ) { }
    }
}