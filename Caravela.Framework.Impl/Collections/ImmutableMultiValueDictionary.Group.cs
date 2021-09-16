// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.Impl.Collections
{
    internal partial class ImmutableMultiValueDictionary<TKey, TValue>
    {
        internal readonly struct Group : IGrouping<TKey, TValue>
        {
            public ImmutableArray<TValue> Items { get; }

            public Group( TKey key, ImmutableArray<TValue> items )
            {
                this.Key = key;
                this.Items = items;
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
        }
    }
}