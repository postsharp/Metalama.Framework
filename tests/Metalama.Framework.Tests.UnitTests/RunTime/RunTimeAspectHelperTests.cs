// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.RunTime;
using System.Collections;
using System.Collections.Generic;
using Xunit;
#if NET5_0_OR_GREATER
using System.Threading.Tasks;
#endif

namespace Metalama.Framework.Tests.UnitTests.RunTime
{
    public sealed class RunTimeAspectHelperTests
    {
        [Fact]
        public void BufferEnumerable()
        {
            var original = ReturnsEnumerable();

            var buffered = original.Buffer();

            CompareEnumerators( ReturnsEnumerable().GetEnumerator(), buffered.GetEnumerator() );
        }

        [Fact]
        public void BufferEnumerableList()
        {
            var list = new List<object>() { 1, "2", 3 };
            var original = (IEnumerable) list;

            var buffered = original.Buffer();

            Assert.Same( buffered, list );
        }

        [Fact]
        public void BufferEnumerator()
        {
            var original = ReturnsEnumerable().GetEnumerator();
            var buffered = original.Buffer();

            CompareEnumerators( ReturnsEnumerable().GetEnumerator(), buffered );
        }

        [Fact]
        public void BufferEnumeratorList()
        {
            var list = new List<object>() { 1, "2", 3 };
            var original = (IEnumerator) list.GetEnumerator();

            var buffered = original.Buffer();

            // We cannot test that no new list was created (the list is not exposed on List.Enumerator)
            // so we have to test the content.

            CompareEnumerators( list.GetEnumerator(), buffered );
        }

        [Fact]
        public void BufferGenericEnumerable()
        {
            var original = ReturnsGenericEnumerable();

            var buffered = original.Buffer();

            CompareGenericEnumerators( ReturnsGenericEnumerable().GetEnumerator(), buffered.GetEnumerator() );
        }

        [Fact]
        public void BufferGenericEnumerableList()
        {
            var list = new List<int>() { 1, 2, 3 };

            var buffered = list.Buffer();

            Assert.Same( buffered, list );
        }

        [Fact]
        public void BufferGenericEnumerator()
        {
            using var original = ReturnsGenericEnumerable().GetEnumerator();
            var buffered = original.Buffer();

            CompareGenericEnumerators( ReturnsGenericEnumerable().GetEnumerator(), buffered );
        }

        [Fact]
        public void BufferGenericEnumeratorList()
        {
            var list = new List<int>() { 1, 2, 3 };

            var buffered = list.GetEnumerator().Buffer();

            // We cannot test that no new list was created (the list is not exposed on List.Enumerator)
            // so we have to test the content.

            CompareGenericEnumerators( list.GetEnumerator(), buffered );
        }

#if NET5_0_OR_GREATER
        [Fact]
        public async Task BufferAsyncEnumerableAsync()
        {
            var original = ReturnsAsyncEnumerableAsync();

            var buffered = await original.BufferAsync();

            await CompareAsyncEnumeratorsAsync( ReturnsAsyncEnumerableAsync().GetAsyncEnumerator(), buffered.GetAsyncEnumerator() );
        }

        [Fact]
        public async Task BufferAsyncEnumerableListAsync()
        {
            var list = new AsyncEnumerableList<int>() { 1, 2, 3 };

            var buffered = await list.BufferAsync();

            Assert.Same( buffered, list );
        }

        [Fact]
        public async Task BufferAsyncEnumeratorAsync()
        {
            var original = ReturnsAsyncEnumerableAsync().GetAsyncEnumerator();
            var buffered = await original.BufferAsync();

            await CompareAsyncEnumeratorsAsync( ReturnsAsyncEnumerableAsync().GetAsyncEnumerator(), buffered );
        }

        [Fact]
        public async Task BufferAsyncEnumeratorListAsync()
        {
            var list = new AsyncEnumerableList<int>() { 1, 2, 3 };

            var buffered = await list.GetAsyncEnumerator().BufferAsync();

            // We cannot test that no new list was created (the list is not exposed on List.Enumerator)
            // so we have to test the content.

            await CompareAsyncEnumeratorsAsync( list.GetAsyncEnumerator(), buffered );
        }

        [Fact]
        public async Task BufferAsyncEnumeratorToListAsync()
        {
            var original = ReturnsAsyncEnumerableAsync().GetAsyncEnumerator();
            var bufferedList = await original.BufferToListAsync();

            await CompareAsyncEnumeratorsAsync( ReturnsAsyncEnumerableAsync().GetAsyncEnumerator(), bufferedList.GetAsyncEnumerator() );

            Assert.Same( bufferedList, bufferedList.GetAsyncEnumerator().Parent );

            var reBufferedList = await bufferedList.GetAsyncEnumerator().BufferToListAsync();

            Assert.Same( bufferedList, reBufferedList );
        }
#endif

        private static void CompareEnumerators( IEnumerator a, IEnumerator b )
        {
            while ( a.MoveNext() )
            {
                Assert.True( b.MoveNext() );
                Assert.Equal( a.Current, b.Current );
            }

            Assert.False( b.MoveNext() );
        }

        private static void CompareGenericEnumerators<T>( IEnumerator<T> a, IEnumerator<T> b )
        {
            while ( a.MoveNext() )
            {
                Assert.True( b.MoveNext() );
                Assert.Equal( a.Current, b.Current );
            }

            Assert.False( b.MoveNext() );
        }

        private static IEnumerable ReturnsEnumerable()
        {
            yield return 1;
            yield return "2";
        }

        private static IEnumerable<int> ReturnsGenericEnumerable()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }

#if NET5_0_OR_GREATER
        private static async Task CompareAsyncEnumeratorsAsync<T>( IAsyncEnumerator<T> a, IAsyncEnumerator<T> b )
        {
            while ( await a.MoveNextAsync() )
            {
                Assert.True( await b.MoveNextAsync() );
                Assert.Equal( a.Current, b.Current );
            }

            Assert.False( await b.MoveNextAsync() );
        }

        private static async IAsyncEnumerable<int> ReturnsAsyncEnumerableAsync()
        {
            await Task.Yield();

            yield return 1;

            await Task.Yield();

            yield return 2;
        }
#endif
    }
}