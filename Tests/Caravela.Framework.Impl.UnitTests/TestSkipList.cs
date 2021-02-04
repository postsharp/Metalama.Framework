using Caravela.Framework.Impl.Collections;
using Caravela.Framework.Impl.Templating;
using System;
using System.Collections.Generic;
using Xunit;

namespace Caravela.Framework.Impl.UnitTests
{
    public class TestSkipList
    {
        // We repeat all tests many times because skip list is a random data structure.
        private const int repeat = 100;
        
        [Theory]
        [Repeat(repeat)]
        public void TestOrdering()
        {
            SkipListIndexedDictionary<int, uint> skipList = new SkipListIndexedDictionary<int, uint>();

            Random random = new Random();

            for (int i = 0; i < 10000; i++)
            {
                skipList.Set(random.Next(), 0xdeadbeef );
            }

            int old = int.MinValue;
            foreach (KeyValuePair<int, uint> pair in skipList)
            {
                Assert.True(old < pair.Key);
                old = pair.Key;
            }
        }

        [Theory]
        [Repeat(repeat)]
        public void TestConflict()
        {
            SkipListIndexedDictionary<int, int> skipList = new SkipListIndexedDictionary<int, int>();
            for ( int i = 0; i < 100; i++ )
            {
                skipList.Add( i, i );
            }

            Assert.Throws<ArgumentException>( () => skipList.Add(20, 20) );
        }

        [Theory]
        [Repeat(repeat)]
        public void TestCount()
        {
            SkipListIndexedDictionary<int, uint> skipList = new SkipListIndexedDictionary<int, uint>();
            Assert.Empty(skipList);
            skipList.Set( 1, 1 );
            Assert.Single(skipList);
            skipList.Set( 1, 1 );
            Assert.Single(skipList);
            skipList.Set( 2, 2 );
            Assert.Equal( 2, skipList.Count );
        }

        [Theory(Skip = "Buggy, so we try not to remove.")]
        [Repeat(repeat)]
        public void TestRemove()
        {
            SkipListIndexedDictionary<int, int> skipList = new SkipListIndexedDictionary<int, int>();
            Assert.Empty(skipList);
            skipList.Add( 1, 1 );
            Assert.Equal( new[] { 1},   skipList.Values );
            skipList.Add( 2, 2 );
            Assert.Equal( new[] { 1, 2 },   skipList.Values );
            skipList.Add( 3, 3 );
            Assert.True( skipList.Remove( 1 ) );
            Assert.True( skipList.Remove( 2 ) );
            Assert.True( skipList.Remove( 3 ) );
            Assert.Empty(skipList);
            
            // Test if we can add an element at the same position.
            skipList.Add( 1, 1 );
            skipList.Add( 2, 2 );
            skipList.Add( 3, 3 );
        }

        [Theory]
        [Repeat(repeat)]
        public void TestIndexOf()
        {
            const int n = 100;
            SkipListIndexedDictionary<int, int> skipList = new SkipListIndexedDictionary<int, int>();
            for ( int i = 0; i < n; i++)
            {
                skipList.Add( i,i );
            }

            for (int j = 0; j < n; j++)
            {
                Assert.Equal(j, skipList.IndexOf(j));
            }
        }

        [Theory]
        [Repeat(repeat)]
        public void TestGetByIndex()
        {
            const int n = 100;
            SkipListIndexedDictionary<int, int> skipList = new SkipListIndexedDictionary<int, int>();
            for (int i = 0; i < n; i++)
            {
                skipList.Add(i, i);
            }

            for (int j = 0; j < n; j++)
            {
                Assert.Equal(j, skipList.GetAt( j ).Key);
            }
        }
    }
}
