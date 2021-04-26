// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using Caravela.Framework.Impl.CodeModel.Links;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class AttributeList : CodeElementList<IAttribute, AttributeLink>, IAttributeList
    {
        public static AttributeList Empty { get; } = new();

        public AttributeList( IEnumerable<AttributeLink> sourceItems, CompilationModel compilation ) : base( sourceItems, compilation ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeList"/> class that contains no element.
        /// </summary>
        private AttributeList() { }
    }
}