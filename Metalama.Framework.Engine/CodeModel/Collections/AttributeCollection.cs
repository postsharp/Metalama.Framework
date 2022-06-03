// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class AttributeCollection : DeclarationCollection<IAttribute, AttributeRef>, IAttributeCollection
    {
        public static AttributeCollection Empty { get; } = new();

        public AttributeCollection( IDeclaration declaringType, IReadOnlyList<AttributeRef> sourceItems ) :
            base( declaringType, sourceItems ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeCollection"/> class that contains no element.
        /// </summary>
        private AttributeCollection() { }
    }
}