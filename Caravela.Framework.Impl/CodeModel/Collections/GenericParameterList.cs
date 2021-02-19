using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class GenericParameterList : CodeElementList<IGenericParameter, CodeElementLink<IGenericParameter>>, IGenericParameterList
    {
        public GenericParameterList(IEnumerable<CodeElementLink<IGenericParameter>> sourceItems, CompilationModel compilation) : base(sourceItems, compilation)
        {
        }
    }
}