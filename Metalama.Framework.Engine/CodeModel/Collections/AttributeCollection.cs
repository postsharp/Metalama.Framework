// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal class AttributeCollection : DeclarationCollection<IAttribute, AttributeRef>, IAttributeCollection
    {
        public static AttributeCollection Empty { get; } = new();

        public AttributeCollection( IDeclaration declaration, IReadOnlyList<AttributeRef> sourceItems ) :
            base( declaration, sourceItems ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeCollection"/> class that contains no element.
        /// </summary>
        private AttributeCollection() { }

        public IEnumerable<IAttribute> OfAttributeType( INamedType type ) => this.GetItems( this.Source ).Where( a => a.Type.Is( type ) );

        public IEnumerable<IAttribute> OfAttributeType( Type type )
        {
            if ( this.ContainingDeclaration == null )
            {
                // The collection is empty.
                return Enumerable.Empty<IAttribute>();
            }

            return this.OfAttributeType( (INamedType) this.ContainingDeclaration!.GetCompilationModel().Factory.GetTypeByReflectionType( type ) );
        }
    }
}