using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class GenericParameterBuilderList : List<GenericParameterBuilder>, IGenericParameterList
    {
        IEnumerator<IGenericParameter> IEnumerable<IGenericParameter>.GetEnumerator() => this.GetEnumerator();

        IGenericParameter IReadOnlyList<IGenericParameter>.this[int index] => this[index];

        // This is to avoid ambiguities in extension methods because this class implements several IEnumerable<>
        public IList<GenericParameterBuilder> AsBuilderList => this;
    }
}