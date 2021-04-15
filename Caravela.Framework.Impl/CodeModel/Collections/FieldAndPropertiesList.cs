// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Code;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class FieldAndPropertiesList : IFieldOrPropertyList
    {
        private readonly IFieldList _fields;
        private readonly IPropertyList _properties;

        public FieldAndPropertiesList( IFieldList fields, IPropertyList properties )
        {
            this._fields = fields;
            this._properties = properties;
        }

        public IEnumerable<IFieldOrProperty> OfName( string name ) => this._fields.OfName( name ).Concat<IFieldOrProperty>( this._properties.OfName( name ) );

        public IEnumerator<IFieldOrProperty> GetEnumerator() => this._fields.Concat<IFieldOrProperty>( this._properties ).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public int Count => this._fields.Count + this._properties.Count;

        public IFieldOrProperty this[ int index ]
        {
            get
            {
                if ( index <= this._fields.Count )
                {
                    return this._fields[index];
                }

                return this._properties[index - this._fields.Count];
            }
        }
    }
}