using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class MethodList : MemberList<IMethod, MemberLink<IMethod>>, IMethodList
    {

        public static MethodList Empty { get; } = new MethodList();
        
        private MethodList() { }

        public MethodList(IEnumerable<MemberLink<IMethod>> sourceItems, CompilationModel compilation) : base(sourceItems, compilation)
        {
        }
    }
}