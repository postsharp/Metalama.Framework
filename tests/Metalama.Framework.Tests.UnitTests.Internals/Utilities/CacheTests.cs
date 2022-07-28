// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Utilities;

public class CacheTests : TestBase
{
    [Fact]
    public void Hit()
    {
        var cache = new FixedCapacityCache<int>( 5 );

        // Cache miss because of non-existent item.
        Assert.Equal( 0, cache.GetOrAdd( "item", _ => true, _ => 0 ) );

        // Cache miss because of invalid predicate.
        Assert.Equal( 1, cache.GetOrAdd( "item", _ => false, _ => 1 ) );

        // Cache hit.
        Assert.Equal( 1, cache.GetOrAdd( "item", _ => true, _ => 2 ) );
    }

    [Fact]
    public async Task ConcurrentCleanUpAsync()
    {
        const int capacity = 500;
        var cache = new FixedCapacityCache<string>( 5 );

        for ( var i = 0; i < capacity * 10; i++ )
        {
            cache.GetOrAdd( $"item{i}", _ => false, s => s );
        }

        Assert.NotNull( cache.CleanUpTask );

        await cache.CleanUpTask!;

        Assert.True( cache.Count == cache.Capacity );
    }

    [Fact]
    public void FileBasedCache()
    {
        var cache = new FileBasedCache<string>();

        using var testContext = this.CreateTestContext();
        var directory = testContext.ProjectOptions.BaseDirectory;
        var fileName = Path.Combine( directory, "test.txt" );

        // Add to cache.
        File.WriteAllText( fileName, "1" );

        Assert.Equal( "1", cache.Get( fileName, File.ReadAllText ) );

        // Cache hit.
        Assert.Equal( "1", cache.Get( fileName, _ => "X" ) );

        // Wait more than the filesystem time resolution.
        Thread.Sleep( 1 );

        // Update the file.
        File.WriteAllText( fileName, "2" );

        // Cache miss. Re-read the file.
        Assert.Equal( "2", cache.Get( fileName, File.ReadAllText ) );
    }
}