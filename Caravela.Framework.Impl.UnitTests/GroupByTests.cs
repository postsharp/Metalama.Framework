using Caravela.Reactive;
using Caravela.Reactive.Collections;
using ComparerExtensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Caravela.Framework.Impl.UnitTests.GroupByTests.TestObserver.EventKind;

namespace Caravela.Framework.Impl.UnitTests
{
    public class GroupByTests
    {
        [Fact]
        public void RegularReactive()
        {
            var source = new ReactiveHashSet<int>();

            var groups = ((IReactiveCollection<int>)source).GroupBy( x => x % 10 );

            Assert.Equal( new object[0], getGroups() );

            var observer = new TestObserver( groups );
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

            var observer = new TestObserver( groups );
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

        internal class TestObserver : IReactiveCollectionObserver<IReactiveGroup<int, int>>
        {
            public enum EventKind
            {
                GroupAdded,
                GroupRemoved,
                GroupReplaced,
                GroupsChanged,
                GroupsInvalidated,
                ItemAdded,
                ItemRemoved,
                ItemReplaced,
                ItemsChanged,
                ItemsInvalidated,
            }

            private readonly List<(EventKind, object)> _events = new();

            private readonly IReactiveCollection<IReactiveGroup<int, int>> _source;

            private int _nextI;

            public TestObserver( IReactiveCollection<IReactiveGroup<int, int>> source )
            {
                this._source = source;
                source.AddObserver( this );
            }

            public void AssertAndClearEvents( params (EventKind, object)[] expected )
            {
                Assert.Equal( expected, this._events );

                this._events.Clear();
            }

            public void ReconnectGroups()
            {
                foreach ( var g in this._source.GetValue() )
                    new GroupObserver( this, g );
            }

            public void Dispose() { }

            public void OnItemAdded( IReactiveSubscription subscription, IReactiveGroup<int, int> item, int newVersion )
            {
                new GroupObserver( this, item );
            }

            public void OnItemRemoved( IReactiveSubscription subscription, IReactiveGroup<int, int> item, int newVersion )
            {
                this._events.Add( (GroupRemoved, item.Key) );
            }

            public void OnItemReplaced( IReactiveSubscription subscription, IReactiveGroup<int, int> oldItem, IReactiveGroup<int, int> newItem, int newVersion )
            {
                this._events.Add( (GroupReplaced, (oldItem.Key, newItem.Key)) );
            }

            public void OnValueChanged( IReactiveSubscription subscription, IEnumerable<IReactiveGroup<int, int>> oldValue, IEnumerable<IReactiveGroup<int, int>> newValue, int newVersion, bool isBreakingChange = false )
            {
                this._events.Add( (GroupsChanged, newValue.Select( g => g.Key ).ToArray()) );
            }

            public void OnValueInvalidated( IReactiveSubscription subscription, bool isBreakingChange )
            {
                this._events.Add( (GroupsInvalidated, isBreakingChange) );
            }

            class GroupObserver : IReactiveCollectionObserver<int>
            {
                private readonly TestObserver _parent;
                private readonly int _i;

                public GroupObserver( TestObserver parent, IReactiveGroup<int, int> source )
                {
                    this._parent = parent;

                    this._i = parent._nextI++;

                    parent._events.Add( (GroupAdded, (this._i, source.Key)) );

                    source.AddObserver( this );
                }

                public void Dispose() { }

                public void OnItemAdded( IReactiveSubscription subscription, int item, int newVersion )
                {
                    this._parent._events.Add( (ItemAdded, (this._i, item)) );
                }

                public void OnItemRemoved( IReactiveSubscription subscription, int item, int newVersion )
                {
                    this._parent._events.Add( (ItemRemoved, (this._i, item)) );
                }

                public void OnItemReplaced( IReactiveSubscription subscription, int oldItem, int newItem, int newVersion )
                {
                    this._parent._events.Add( (ItemReplaced, (this._i, (oldItem, newItem))) );
                }

                public void OnValueChanged( IReactiveSubscription subscription, IEnumerable<int> oldValue, IEnumerable<int> newValue, int newVersion, bool isBreakingChange = false )
                {
                    this._parent._events.Add( (ItemsChanged, this._i) );
                }

                public void OnValueInvalidated( IReactiveSubscription subscription, bool isBreakingChange )
                {
                    this._parent._events.Add( (ItemsInvalidated, (this._i, isBreakingChange)) );
                }
            }
        }
    }
}
