// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Collections
{
    public sealed partial class ImmutableDictionaryOfArray<TKey, TValue>
    {
        internal readonly struct Group : IGrouping<TKey, TValue>, IEquatable<Group>
        {
            private readonly IEqualityComparer<TKey> _keyComparer;

            public ImmutableArray<TValue> Items { get; }

            public Group( TKey key, ImmutableArray<TValue> items, IEqualityComparer<TKey> keyComparer )
            {
#if DEBUG
                if ( items.IsDefault )
                {
                    throw new ArgumentNullException( nameof(items) );
                }
#endif

                this.Key = key;
                this.Items = items;
                this._keyComparer = keyComparer;
            }

            public TKey Key { get; }

            public IEnumerator<TValue> GetEnumerator()
            {
                foreach ( var value in this.Items )
                {
                    yield return value;
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

            public Group Add( TValue value ) => new( this.Key, this.Items.Add( value ), this._keyComparer );

            public override string ToString() => $"Key={this.Key}, Items={this.Items.Length}";

            public bool Equals( Group other ) => this._keyComparer.Equals( this.Key, other.Key );

            public override bool Equals( object? obj ) => obj is Group other && this.Equals( other );

            public override int GetHashCode() => this._keyComparer.GetHashCode( this.Key );

            public static bool operator ==( Group left, Group right ) => left.Equals( right );

            public static bool operator !=( Group left, Group right ) => !left.Equals( right );
        }
    }
}