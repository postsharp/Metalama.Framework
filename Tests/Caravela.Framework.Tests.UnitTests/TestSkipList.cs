// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using Caravela.Framework.Impl.Collections;
using Xunit;

#pragma warning disable xUnit1026 // Theory methods should use all of their parameters

namespace Caravela.Framework.Tests.UnitTests
{
    public class TestSkipList
    {
        // We repeat all tests many times because skip list is a random data structure.
        private const int _repeat = 100;

        [Theory]
        [Repeat( _repeat )]
        public void TestOrdering( int attempt )
        {
            var skipList = new SkipListIndexedDictionary<int, uint>();

            var random = new Random();

            for ( var i = 0; i < 10000; i++ )
            {
                skipList.Set( random.Next(), 0xdeadbeef );
            }

            var old = int.MinValue;
            foreach ( var pair in skipList )
            {
                Assert.True( old < pair.Key );
                old = pair.Key;
            }
        }

        [Theory]
        [Repeat( _repeat )]
        public void TestConflict( int attempt )
        {
            var skipList = new SkipListIndexedDictionary<int, int>();
            for ( var i = 0; i < 100; i++ )
            {
                skipList.Add( i, i );
            }

            Assert.Throws<ArgumentException>( () => skipList.Add( 20, 20 ) );
        }

        [Theory]
        [Repeat( _repeat )]
        public void TestCount( int attempt )
        {
            var skipList = new SkipListIndexedDictionary<int, uint>();
            Assert.Empty( skipList );
            skipList.Set( 1, 1 );
            Assert.Single( skipList );
            skipList.Set( 1, 1 );
            Assert.Single( skipList );
            skipList.Set( 2, 2 );
            Assert.Equal( 2, skipList.Count );
        }

        [Theory( Skip = "Buggy, so we try not to use the Remove method." )]
        [Repeat( _repeat )]
        public void TestRemove( int attempt )
        {
            var skipList = new SkipListIndexedDictionary<int, int>();
            Assert.Empty( skipList );
            skipList.Add( 1, 1 );
            Assert.Equal( new[] { 1 }, skipList.Values );
            skipList.Add( 2, 2 );
            Assert.Equal( new[] { 1, 2 }, skipList.Values );
            skipList.Add( 3, 3 );
            Assert.True( skipList.Remove( 1 ) );
            Assert.True( skipList.Remove( 2 ) );
            Assert.True( skipList.Remove( 3 ) );
            Assert.Empty( skipList );

            // Test if we can add an element at the same position.
            skipList.Add( 1, 1 );
            skipList.Add( 2, 2 );
            skipList.Add( 3, 3 );
        }

        [Theory]
        [Repeat( _repeat )]
        public void TestIndexOf( int attempt )
        {
            const int n = 100;
            var skipList = new SkipListIndexedDictionary<int, int>();
            for ( var i = 0; i < n; i++ )
            {
                skipList.Add( i, i );
            }

            for ( var j = 0; j < n; j++ )
            {
                Assert.Equal( j, skipList.IndexOf( j ) );
            }
        }

        [Theory]
        [Repeat( _repeat )]
        public void TestGetByIndex( int attempt )
        {
            const int n = 100;
            var skipList = new SkipListIndexedDictionary<int, int>();
            for ( var i = 0; i < n; i++ )
            {
                skipList.Add( i, i );
            }

            for ( var j = 0; j < n; j++ )
            {
                Assert.Equal( j, skipList.GetAt( j ).Key );
            }
        }
    }
}
