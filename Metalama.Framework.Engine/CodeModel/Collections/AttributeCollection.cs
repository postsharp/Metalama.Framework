// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.CodeModel.References;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.CodeModel.Collections
{
    internal sealed class AttributeCollection : DeclarationCollection<IAttribute, AttributeRef>, IAttributeCollection
    {
        public static AttributeCollection Empty { get; } = new();

        public AttributeCollection( IDeclaration declaration, IReadOnlyList<AttributeRef> sourceItems ) :
            base( declaration, sourceItems ) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeCollection"/> class that contains no element.
        /// </summary>
        private AttributeCollection() { }

        IEnumerable<IAttribute> IAttributeCollection.OfAttributeType( INamedType type ) => this.OfAttributeType( type );

        IEnumerable<IAttribute> IAttributeCollection.OfAttributeType( INamedType type, ConversionKind conversionKind ) => this.OfAttributeType( type, conversionKind );

        private IEnumerable<IAttribute> OfAttributeType( INamedType type, ConversionKind conversionKind = ConversionKind.Default ) => this.GetItems( this.Source ).Where( a => a.Type.Is( type, conversionKind ) );

        IEnumerable<IAttribute> IAttributeCollection.OfAttributeType( Type type ) => this.OfAttributeType( type );

        IEnumerable<IAttribute> IAttributeCollection.OfAttributeType( Type type, ConversionKind conversionKind ) => this.OfAttributeType( type, conversionKind );

        private IEnumerable<IAttribute> OfAttributeType( Type type, ConversionKind conversionKind = ConversionKind.Default )
        {
            if ( this.ContainingDeclaration == null )
            {
                // The collection is empty.
                return Enumerable.Empty<IAttribute>();
            }

            return this.OfAttributeType( (INamedType) this.ContainingDeclaration!.GetCompilationModel().Factory.GetTypeByReflectionType( type ), conversionKind );
        }

        bool IAttributeCollection.Any( INamedType type ) => this.Any( type );

        bool IAttributeCollection.Any( INamedType type, ConversionKind conversionKind ) => this.Any( type, conversionKind );

        private bool Any( INamedType type, ConversionKind conversionKind = ConversionKind.Default ) => this.GetItems( this.Source ).Any( a => a.Type.Is( type, conversionKind ) );

        bool IAttributeCollection.Any( Type type ) => this.Any( type );

        bool IAttributeCollection.Any( Type type, ConversionKind conversionKind ) => this.Any( type, conversionKind );

        private bool Any( Type type, ConversionKind conversionKind = ConversionKind.Default )
        {
            if ( this.ContainingDeclaration == null )
            {
                // The collection is empty.
                return false;
            }

            return this.Any( (INamedType) this.ContainingDeclaration!.GetCompilationModel().Factory.GetTypeByReflectionType( type ), conversionKind );
        }
    }
}