using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class NamedTypeList : MemberList<INamedType, MemberLink<INamedType>>, INamedTypeList
    {
        public NamedTypeList(IEnumerable<MemberLink<INamedType>> sourceItems, CompilationModel compilation) : base(sourceItems, compilation)
        {
        }
    }
}