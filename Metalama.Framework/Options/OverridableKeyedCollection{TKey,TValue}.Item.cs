// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Serialization;

namespace Metalama.Framework.Options;

public partial class OverridableKeyedCollection<TKey, TValue>
{
    protected internal readonly struct Item : ICompileTimeSerializable
    {
        public TValue? Value { get; }

        public bool IsEnabled { get; }

        public Item( TValue? value, bool isEnabled = true )
        {
            this.Value = value;
            this.IsEnabled = isEnabled;
        }

#pragma warning disable SA1101

        // ReSharper disable once MemberHidesStaticFromOuterClass
        [UsedImplicitly]
        private sealed class Serializer : ValueTypeSerializer<Item>
        {
            public override void SerializeObject( Item obj, IArgumentsWriter constructorArguments )
            {
                constructorArguments.SetValue( nameof(Value), obj.Value );
                constructorArguments.SetValue( nameof(IsEnabled), obj.IsEnabled );
            }

            public override Item DeserializeObject( IArgumentsReader constructorArguments )
            {
                return new Item(
                    constructorArguments.GetValue<TValue>( nameof(Value) ),
                    constructorArguments.GetValue<bool>( nameof(IsEnabled) ) );
            }
        }
    }
}