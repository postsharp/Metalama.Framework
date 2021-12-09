// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class AttributeList : DeclarationList<IAttribute, AttributeRef>, IAttributeList
    {
        public static AttributeList Empty { get; } = new();

        public AttributeList( IDeclaration containingDeclaration, IEnumerable<AttributeRef> sourceItems ) : base( containingDeclaration, sourceItems ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeList"/> class that contains no element.
        /// </summary>
        private AttributeList() { }
    }
}