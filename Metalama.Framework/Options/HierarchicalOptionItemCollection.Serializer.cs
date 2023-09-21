// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Serialization;
using System;
using System.Collections.Immutable;

namespace Metalama.Framework.Options;

public sealed partial class HierarchicalOptionItemCollection<T>
{
    [UsedImplicitly]
    private sealed class Serializer : ReferenceTypeSerializer
    {
        public override object CreateInstance( Type type, IArgumentsReader constructorArguments )
        {
            var clear = constructorArguments.GetValue<bool>( "clear" );

            return new HierarchicalOptionItemCollection<T>( null!, clear );
        }

        public override void SerializeObject( object obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
        {
            var collection = (HierarchicalOptionItemCollection<T>) obj;
            constructorArguments.SetValue( "clear", collection._clear );
            initializationArguments.SetValue( "items", collection._items );
        }

        public override void DeserializeFields( object obj, IArgumentsReader initializationArguments )
        {
            var collection = (HierarchicalOptionItemCollection<T>) obj;

            collection._items = initializationArguments.GetValue<ImmutableDictionary<object, Item>>( "items" )!;
        }
    }
}