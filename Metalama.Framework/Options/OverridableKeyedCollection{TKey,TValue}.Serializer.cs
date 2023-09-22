// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Serialization;
using System.Collections.Immutable;

namespace Metalama.Framework.Options;

public partial class OverridableKeyedCollection<TKey, TValue>
{
    [UsedImplicitly]
    private sealed class Serializer : ReferenceTypeSerializer<OverridableKeyedCollection<TKey, TValue>>
    {
        public override OverridableKeyedCollection<TKey, TValue> CreateInstance( IArgumentsReader constructorArguments )
        {
            var clear = constructorArguments.GetValue<bool>( "clear" );

            return new OverridableKeyedCollection<TKey, TValue>( null!, clear );
        }

        public override void SerializeObject(
            OverridableKeyedCollection<TKey, TValue> obj,
            IArgumentsWriter constructorArguments,
            IArgumentsWriter initializationArguments )
        {
            constructorArguments.SetValue( "clear", obj._clear );
            initializationArguments.SetValue( "items", obj._dictionary );
        }

        public override void DeserializeFields( OverridableKeyedCollection<TKey, TValue> obj, IArgumentsReader initializationArguments )
        {
            obj._dictionary = initializationArguments.GetValue<ImmutableDictionary<TKey, Item>>( "items" )!;
        }
    }
}