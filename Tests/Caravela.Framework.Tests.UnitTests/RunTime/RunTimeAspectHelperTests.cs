// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.RunTime;
using System.Collections;
using System.Collections.Generic;
using Xunit;

#if NET5_0
using System.Threading.Tasks;
#endif

namespace Caravela.Framework.Tests.UnitTests.RunTime
{
    public class RunTimeAspectHelperTests
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

#if NET5_0
        [Fact]
        public async Task BufferAsyncEnumerable()
        {
            var original = ReturnsAsyncEnumerable();

            var buffered = await original.BufferAsync();

            await CompareAsyncEnumerators( ReturnsAsyncEnumerable().GetAsyncEnumerator(), buffered.GetAsyncEnumerator() );
        }

        [Fact]
        public async Task BufferAsyncEnumerableList()
        {
            var list = new AsyncEnumerableList<int>() { 1, 2, 3 };

            var buffered = await list.BufferAsync();

            Assert.Same( buffered, list );
        }

        [Fact]
        public async Task BufferAsyncEnumerator()
        {
            var original = ReturnsAsyncEnumerable().GetAsyncEnumerator();
            var buffered = await original.BufferAsync();

            await CompareAsyncEnumerators( ReturnsAsyncEnumerable().GetAsyncEnumerator(), buffered );
        }

        [Fact]
        public async Task BufferAsyncEnumeratorList()
        {
            var list = new AsyncEnumerableList<int>() { 1, 2, 3 };

            var buffered = await list.GetAsyncEnumerator().BufferAsync();

            // We cannot test that no new list was created (the list is not exposed on List.Enumerator)
            // so we have to test the content.

            await CompareAsyncEnumerators( list.GetAsyncEnumerator(), buffered );
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

#if NET5_0
        private static async Task CompareAsyncEnumerators<T>( IAsyncEnumerator<T> a, IAsyncEnumerator<T> b )
        {
            while ( await a.MoveNextAsync() )
            {
                Assert.True( await b.MoveNextAsync() );
                Assert.Equal( a.Current, b.Current );
            }

            Assert.False( await b.MoveNextAsync() );
        }

        private static async IAsyncEnumerable<int> ReturnsAsyncEnumerable()
        {
            await Task.Yield();

            yield return 1;

            await Task.Yield();

            yield return 2;
        }
#endif
    }
}