using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class PropertyList : MemberList<IProperty, MemberLink<IProperty>>, IPropertyList
    {
        public PropertyList( IEnumerable<MemberLink<IProperty>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }
    }
}