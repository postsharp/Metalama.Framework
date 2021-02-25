using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class ConstructorList : MemberList<IConstructor, MemberLink<IConstructor>>, IConstructorList
    {
        public ConstructorList( IEnumerable<MemberLink<IConstructor>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }
    }
}