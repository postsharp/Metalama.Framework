using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class AttributeList : CodeElementList<IAttribute, AttributeLink>, IAttributeList
    {
        public static AttributeList Empty { get; } = new AttributeList();

        public AttributeList( IEnumerable<AttributeLink> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeList"/> class that contains no element.
        /// </summary>
        private AttributeList() : base()
        {
        }
    }
}