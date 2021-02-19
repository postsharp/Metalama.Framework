using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class ParameterBuilderList : List<ParameterBuilder>, IParameterList
    {
        IEnumerator<IParameter> IEnumerable<IParameter>.GetEnumerator() => this.GetEnumerator();

        IParameter IReadOnlyList<IParameter>.this[ int index ] => this[index];
        
        
        // This is to avoid ambiguities in extension methods because this class implements several IEnumerable<>
        public IList<ParameterBuilder> AsBuilderList => this;
    }
}