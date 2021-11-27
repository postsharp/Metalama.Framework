// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Immutable;
using System.Linq;

namespace Caravela.Framework.LinqPad
{
    internal class GroupingFacade<TKey, TItems>
    {
        public GroupingFacade( IGrouping<TKey, TItems> underlying )
        {
            this.Key = underlying.Key;
            this.Items = underlying.ToImmutableArray();
        }

        public TKey Key { get; }

        public ImmutableArray<TItems> Items { get; }

        public override string ToString() => $"{this.Key} => {this.Items.Length} item(s)";
    }
}