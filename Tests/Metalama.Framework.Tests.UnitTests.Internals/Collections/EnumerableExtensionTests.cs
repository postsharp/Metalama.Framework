// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Code.Collections;
using Metalama.Framework.Engine.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace Metalama.Framework.Tests.UnitTests.Collections
{
    public class EnumerableExtensionTests
    {
        [Fact]
        public void AddRange()
        {
            IList<int> l = new List<int>();
            l.AddRange( new[] { 1, 2, 3 } );
            Assert.Equal( 3, l.Count );
        }

        [Fact]
        public void SelectManyRecursiveOnItem()
        {
            Node a = new();
            Node b = new();
            Node c = new( a, b );
            Node d = new( c, b );

            Assert.Equal( new[] { a, b, c }, d.SelectManyRecursive( n => n.Children, throwOnDuplicate: false ).OrderBy( o => o.Id ) );
            Assert.Equal( new[] { a, b, c, d }, d.SelectManyRecursive( n => n.Children, includeThis: true, throwOnDuplicate: false ).OrderBy( o => o.Id ) );
            Assert.Throws<InvalidOperationException>( () => d.SelectManyRecursive( n => n.Children, throwOnDuplicate: true ) );
        }

        [Fact]
        public void SelectManyRecursiveOnList()
        {
            Node a = new();
            Node b = new();
            Node c = new( a, b );
            Node d = new( c, b );
            var list = new[] { d, a };

            Assert.Equal( new[] { a, b, c, d }, list.SelectManyRecursive( n => n.Children, throwOnDuplicate: false ).OrderBy( o => o.Id ) );
            Assert.Throws<InvalidOperationException>( () => list.SelectManyRecursive( n => n.Children, throwOnDuplicate: true ) );
        }

        [Fact]
        public void ConcatEmptyList()
        {
            var emptyArray = Array.Empty<int>();
            Assert.Single( emptyArray.Concat( 1 ), 1 );
        }

        private class Node
        {
            public int Id { get; } = Interlocked.Increment( ref _nextId );

            private static int _nextId;

            public IReadOnlyList<Node> Children { get; }

            public Node( params Node[] children )
            {
                this.Children = children;
            }
        }
    }
}