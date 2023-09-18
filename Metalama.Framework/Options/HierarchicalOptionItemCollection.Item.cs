// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Serialization;

namespace Metalama.Framework.Options;

public partial class HierarchicalOptionItemCollection<T>
{
    private readonly struct Item : ICompileTimeSerializable
    {
        public T? Value { get; }

        public bool IsEnabled { get; }

        public Item( T? value, bool isEnabled = true )
        {
            this.Value = value;
            this.IsEnabled = isEnabled;
        }
    }
}