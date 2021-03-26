// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Caravela.Framework.Code;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class NamedArgumentsList : Collection<KeyValuePair<string, TypedConstant>>, INamedArgumentList
    {
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public NamedArgumentsList( IEnumerable<KeyValuePair<string, TypedConstant>> source )
        {
            foreach ( var item in source )
            {
                this.Add( item );
            }
        }

        public NamedArgumentsList()
        {
        }

        public bool TryGetByName( string name, out TypedConstant value )
        {
            var named = this.Items.Where( p => p.Key == name );
            using var enumerator = named.GetEnumerator();
            if ( enumerator.MoveNext() )
            {
                value = enumerator.Current.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public object? GetValue( string name )
        {
            _ = this.TryGetByName( name, out var value );
            return value.Value;
        }
    }
}