using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class GenericParameterList : CodeElementList<IGenericParameter, CodeElementLink<IGenericParameter>>, IGenericParameterList
    {
        public GenericParameterList( IEnumerable<CodeElementLink<IGenericParameter>> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }
    }
}