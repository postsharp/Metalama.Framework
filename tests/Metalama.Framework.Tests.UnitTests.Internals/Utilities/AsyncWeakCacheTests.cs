// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities.Caching;
using Metalama.Testing.UnitTesting;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

#pragma warning disable VSTHRD200

namespace Metalama.Framework.Tests.UnitTests.Utilities;

public class AsyncWeakCacheTests : UnitTestSuite
{
    [Fact]
    public void Hit()
    {
        var cache = new AsyncWeakCache<object, int>();
        var key1 = new object();

        Assert.False( cache.TryGetValue( key1, out _ ) );
        Assert.True( cache.TryAdd( key1, 1 ) );
        Assert.True( cache.TryGetValue( key1, out var value1 ) );
        Assert.Equal( 1, value1 );
        Assert.Equal( 1, cache.GetOrAdd( key1, _ => 2 ) );
    }

    [Fact]
    public async Task HitAsync()
    {
        var cache = new AsyncWeakCache<object, int>();
        var key1 = new object();

        Assert.False( cache.TryGetValue( key1, out _ ) );
        Assert.True( await cache.TryAddAsync( key1, 1 ) );
        Assert.True( cache.TryGetValue( key1, out var value1 ) );
        Assert.Equal( 1, value1 );
        Assert.Equal( 1, await cache.GetOrAddAsync( key1, ( _, _ ) => new ValueTask<int>( 2 ) ) );
    }

    [Fact]
    public void Recursion()
    {
        var cache = new AsyncWeakCache<object, int>();
        var key = new object();
        Assert.Equal( 1, cache.GetOrAdd( key, _ => cache.GetOrAdd( key, _ => 1 ) + 1 ) );
    }

    [Fact]
    public async Task RecursionAsync()
    {
        var cache = new AsyncWeakCache<object, int>();
        var key = new object();

        Assert.Equal(
            1,
            await cache.GetOrAddAsync(
                key,
                async ( _, ct ) => await cache.GetOrAddAsync( key, ( _, _ ) => new ValueTask<int>( 1 ), ct ) + 1 ) );
    }

    [Fact]
    public async Task Concurrency()
    {
        var cache = new AsyncWeakCache<object, object>();

        // Test that the add method does not run concurrently.
        var keyLockMap = Enumerable.Range( 0, 20 ).ToDictionary( _ => new object(), _ => new object() );

        var tasks = Enumerable.Range( 0, Environment.ProcessorCount )
            .Select(
                _ => Task.Run(
                    () =>
                    {
                        for ( var n = 0; n < 10; n++ )
                        {
                            foreach ( var pair in keyLockMap )
                            {
                                cache.GetOrAdd(
                                    pair.Key,
                                    k =>
                                    {
                                        // Here is the assertion: the Func should not be executed by two threads at a time.
                                        Assert.True( Monitor.TryEnter( pair.Value ) );
                                        Thread.Sleep( 1 );
                                        Monitor.Exit( pair.Value );

                                        return k;
                                    } );
                            }
                        }
                    } ) );

        await Task.WhenAll( tasks );
    }

    [Fact]
    public async Task ConcurrencyAsync()
    {
        var cache = new AsyncWeakCache<object, object>();

        // Test that the add method does not run concurrently.
        var keyLockMap = Enumerable.Range( 0, 20 ).ToDictionary( _ => new object(), _ => new object() );

        var tasks = Enumerable.Range( 0, Environment.ProcessorCount )
            .Select(
                _ => Task.Run(
                    async () =>
                    {
                        for ( var n = 0; n < 10; n++ )
                        {
                            foreach ( var pair in keyLockMap )
                            {
                                await cache.GetOrAddAsync(
                                    pair.Key,
                                    ( k, _ ) =>
                                    {
                                        // Here is the assertion: the Func should not be executed by two threads at a time.
                                        Assert.True( Monitor.TryEnter( pair.Value ) );
                                        Thread.Sleep( 1 );
                                        Monitor.Exit( pair.Value );

                                        return new ValueTask<object>( k );
                                    } );
                            }
                        }
                    } ) );

        await Task.WhenAll( tasks );
    }
}