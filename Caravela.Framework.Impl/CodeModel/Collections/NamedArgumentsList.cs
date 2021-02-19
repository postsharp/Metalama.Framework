using Caravela.Framework.Code;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Caravela.Framework.Impl.CodeModel.Collections
{
    internal class NamedArgumentsList : Collection<KeyValuePair<string, object>>, INamedArgumentList
    {
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        public NamedArgumentsList( IEnumerable<KeyValuePair<string, object?>> source ) 
        {
            foreach ( var item in source )
            {
                this.Add( item );
            }
        }

        public NamedArgumentsList() { }

        public bool TryGetByName( string name, out object? value )
        {
            var named = this.Items.Where( p => p.Key == name );
            var enumerator = named.GetEnumerator();
            if ( enumerator.MoveNext() )
            {
                value = enumerator.Current.Value;
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public object? GetByName( string name )
        {
            _ = this.TryGetByName( name, out var value );
            return value;
        }
    }
}