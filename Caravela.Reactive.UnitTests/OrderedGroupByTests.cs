using Caravela.Reactive;
using Caravela.Reactive.Sources;
using ComparerExtensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Caravela.Reactive.UnitTests.TestGroupObserver.EventKind;

namespace Caravela.Reactive.UnitTests
{
    public class OrderedGroupByTests
    {
        [Fact]
        public void TestObserver()
        {
            var source = new ReactiveHashSet<int>();

            var groups = ((IReactiveCollection<int>)source).GroupBy( x => x % 10 );

            Assert.Equal( new object[0], getGroups() );

            var observer = new TestGroupObserver( groups );
            observer.AssertAndClearEvents();

            source.Add( 1 );
            observer.AssertAndClearEvents( (GroupAdded, (0, 1)), (ItemAdded, (0, 1)), (ItemsChanged, 0), (GroupsInvalidated, false) );
            Assert.Equal( new[] { new[] { 1 } }, getGroups() );

            source.Add( 11 );
            observer.AssertAndClearEvents( (ItemAdded, (0, 11)), (ItemsChanged, 0), (GroupsInvalidated, false) );
            Assert.Equal( new[] { new[] { 1, 11 } }, getGroups() );

            source.Add( 12 );
            observer.AssertAndClearEvents( (GroupAdded, (1, 2)), (ItemAdded, (1, 12)), (ItemsChanged, 1), (GroupsInvalidated, false) );
            Assert.Equal( new[] { new[] { 1, 11 }, new[] { 12 } }, getGroups() );

            IEnumerable<IEnumerable<int>> getGroups() => groups.GetValue().Select( g => g.GetValue() );
        }


        [Fact]
        public void OrderedImmutable()
        {
            var source = new[] { 1, 11, 12, 21 }.ToImmutableReactive();

            var groups = source.OrderedGroupBy( KeyComparer<int>.OrderBy(i => 0), i => i % 10 ).GetValue();

            Assert.Equal( new[] { new[] { 1, 11 }, new[] { 12 }, new[] { 21 } }, groups.Select( g => g.GetValue() ) );
        }

        [Fact]
        public void OrderedReactive()
        {
            var source = new ReactiveHashSet<(int order, int value)>();

            var groups = source.OrderedGroupBy( KeyComparer<(int order, int value)>.OrderBy( x => x.order ), x => x.value % 10, x => x.value );
            var itemsInGroups = groups.SelectMany( x => x );

            Assert.Equal( new object[0], getGroups() );
            Assert.Equal( new int[0], itemsInGroups.GetValue() );

            var observer = new TestGroupObserver( groups );
            observer.AssertAndClearEvents();

            // _ _ 1
            source.Add( (3, 1) );
            observer.AssertAndClearEvents( (GroupsInvalidated, true) );
            Assert.Equal( new[] { new[] { 1 } }, getGroups() );
            Assert.Equal( new[] { 1 }, itemsInGroups.GetValue() );

            // this can't be done automatically in response to a breaking change, because that causes LockRecursionException
            observer.ReconnectGroups();
            observer.AssertAndClearEvents( (GroupAdded, (0, 1)) );

            // 11 _ 1
            source.Add( (1, 11) );
            observer.AssertAndClearEvents( (ItemAdded, (0, 11)), (ItemsChanged, 0), (GroupsInvalidated, false) );
            Assert.Equal( new[] { new[] { 1, 11 } }, getGroups() );
            Assert.Equal( new[] { 1, 11 }, itemsInGroups.GetValue() );

            // 11 12 1
            source.Add( (2, 12) );
            observer.AssertAndClearEvents( (GroupsInvalidated, true) );
            Assert.Equal( new[] { new[] { 11 }, new[] { 12 }, new[] { 1 } }, getGroups() );
            Assert.Equal( new[] { 11, 12, 1 }, itemsInGroups.GetValue() );

            IEnumerable<IEnumerable<int>> getGroups() => groups.GetValue().Select( g => g.GetValue() );
        }
    }
}
