// unset

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class ParameterList : CodeElementList<IParameter, CodeElementLink<IParameter>>, IParameterList
    {
        public ParameterList(IEnumerable<CodeElementLink<IParameter>> sourceItems, CompilationModel compilation) : base(sourceItems, compilation)
        {
        }

        private ParameterList() { }

        public static ParameterList Empty { get; } = new ParameterList();

    }
}