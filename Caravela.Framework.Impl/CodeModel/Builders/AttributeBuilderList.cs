using System.Collections.Generic;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class AttributeBuilderList : List<AttributeBuilder>, IAttributeList
    {
        IEnumerator<IAttribute> IEnumerable<IAttribute>.GetEnumerator() => this.GetEnumerator();

        IAttribute IReadOnlyList<IAttribute>.this[int index] => this[index];
    }
}