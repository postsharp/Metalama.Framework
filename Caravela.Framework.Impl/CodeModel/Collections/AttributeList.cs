// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class AttributeList : CodeElementList<IAttribute, AttributeLink>, IAttributeList
    {
        public static AttributeList Empty { get; } = new AttributeList();

        public AttributeList( ICodeElement containingElement, IEnumerable<AttributeLink> sourceItems ) : base( containingElement, sourceItems )
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeList"/> class that contains no element.
        /// </summary>
        private AttributeList()
        {
        }
    }
}