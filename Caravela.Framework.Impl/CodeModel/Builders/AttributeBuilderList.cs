using Caravela.Framework.Code;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Builders
{
    internal class AttributeBuilderList : List<AttributeBuilder>, IAttributeList
    {
        IEnumerator<IAttribute> IEnumerable<IAttribute>.GetEnumerator() => this.GetEnumerator();

        IAttribute IReadOnlyList<IAttribute>.this[ int index ] => this[index];
    }
}