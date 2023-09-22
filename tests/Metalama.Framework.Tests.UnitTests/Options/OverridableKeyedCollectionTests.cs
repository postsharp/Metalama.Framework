// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Options;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Options;

public sealed class OverridableKeyedCollectionTests
{
    [Fact]
    public void Add()
    {
        Assert.Single( OverridableKeyedCollection.AddOrOverride<string, Item>( new Item( "Item" ) ) );
    }

    [Fact]
    public void Replace()
    {
        var item = Assert.Single(
            OverridableKeyedCollection.AddOrOverride<string, Item>( new Item( "Item", "New" ) ).AddOrOverride( new Item( "Item", "Replaced" ) ) );

        Assert.Equal( "Replaced", item.Value );
    }

    [Fact]
    public void Remove()
    {
        var item = OverridableKeyedCollection.AddOrOverride<string, Item>( new Item( "Item" ) ).Remove( "Item" );
        Assert.Empty( item );
    }

    [Fact]
    public void ClearThenAdd()
    {
        var item = Assert.Single(
            OverridableKeyedCollection.AddOrOverride<string, Item>( new Item( "Item" ) ).Clear().AddOrOverride( new Item( "Item", "New" ) ) );

        Assert.Equal( "New", item.Value );
    }

    [Fact]
    public void OverrideWithClear()
    {
        var collection1 = OverridableKeyedCollection.AddOrOverride<string, Item>( new Item( "Key" ) );
        var collection2 = OverridableKeyedCollection.Clear<string, Item>();
        var merged = collection1.OverrideWith( collection2, default );
        Assert.Empty( merged );
    }

    [Fact]
    public void OverrideWithUpdate()
    {
        var collection1 = OverridableKeyedCollection.AddOrOverride<string, Item>( new Item( "Key" ) );
        var collection2 = OverridableKeyedCollection.AddOrOverride<string, Item>( new Item( "Key", "Updated" ) );
        var merged = collection1.OverrideWith( collection2, default );
        Assert.Equal( "Updated", Assert.Single( merged ).Value );
    }

    private class Item : IOverridableKeyedCollectionItem<string>
    {
        public object OverrideWith( object overridingObject, in OverrideContext context )
            => new Item( this.Key, ((Item) overridingObject).Value ?? this.Value );

        public string Key { get; }

        public string? Value { get; }

        public Item( string key, string? value = null )
        {
            this.Key = key;
            this.Value = value;
        }
    }
}