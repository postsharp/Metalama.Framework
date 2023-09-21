// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using JetBrains.Annotations;
using Metalama.Framework.Options;
using Metalama.Framework.Serialization;
using System.Linq;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.LamaSerialization;

public sealed class HierarchicalOptionsSerializersTest : SerializationTestsBase
{
    [Fact]
    public void Collection()
    {
        var collection = new HierarchicalOptionItemCollection<MyItem>( new MyItem( "TheKey" ) );
        var roundtrip = this.SerializeDeserialize( collection );
        Assert.Single( roundtrip );
        Assert.Equal( "TheKey", roundtrip.Single().Key );
    }

    private sealed class MyItem : IHierarchicalOptionItem
    {
        public MyItem( string key )
        {
            this.Key = key;
        }

        public string Key { get; }

        public object OverrideWith( object overridingObject, in OverrideContext context ) => this;

        public object GetKey() => this.Key;

        [UsedImplicitly]
        private sealed class Serializer : ReferenceTypeSerializer<MyItem>
        {
            public override MyItem CreateInstance( IArgumentsReader constructorArguments ) => new( constructorArguments.GetValue<string>( "Key" )! );

            public override void SerializeObject( MyItem obj, IArgumentsWriter constructorArguments, IArgumentsWriter initializationArguments )
            {
                constructorArguments.SetValue( "Key", obj.Key );
            }

            public override void DeserializeFields( MyItem obj, IArgumentsReader initializationArguments ) { }
        }
    }
}