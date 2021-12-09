// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Metalama.Reactive.Implementation;
using MoreLinq;

namespace Metalama.Reactive.Operators
{
    internal class OrderedGroupByOperator<TSource, TKey, TElement> : ReactiveCollectionOperator<TSource, IReactiveGroup<TKey, TElement>>,
        IGroupByOperator<TKey, TElement>
    {
        private readonly IComparer<TSource> _sourceComparer;
        private readonly IEqualityComparer<TKey> _equalityComparer;
        private readonly Func<TSource, ReactiveCollectorToken, TKey> _getKeyFunc;
        private readonly Func<TSource, TElement> _getElementFunc;
        private ImmutableArray<Group<TKey, TElement>> _groups;
        private ImmutableDictionary<TSource, Group<TKey, TElement>> _sourceMap;

        public OrderedGroupByOperator(
            IReactiveCollection<TSource> source,
            IComparer<TSource> sourceComparer,
            Func<TSource, TKey> getKeyFunc,
            Func<TSource, TElement> getElementFunc,
            IEqualityComparer<TKey>? equalityComparer ) : base( source )
        {
            this._sourceComparer = sourceComparer;
            this._equalityComparer = equalityComparer ?? EqualityComparer<TKey>.Default;
            this._getKeyFunc = ReactiveCollectorToken.WrapWithDefaultToken( getKeyFunc );
            this._getElementFunc = getElementFunc;
            this._groups = ImmutableArray<Group<TKey, TElement>>.Empty;
            this._sourceMap = ImmutableDictionary<TSource, Group<TKey, TElement>>.Empty;
        }

        public override bool IsMaterialized => true;

        void IGroupByOperator<TKey, TElement>.EnsureSubscribedToSource() => this.EnsureSubscribedToSource();

        private void AddItem( TSource item, in IncrementalUpdateToken updateToken )
        {
            var keyGroups = this.ComputeKeyGroups( this.Source.GetValue( this.ObserverToken ) );

            // TODO: process added groups incrementally
            if ( !this.AreKeysSame( keyGroups ) )
            {
                updateToken.SetBreakingChange( breaksObservers: true );
                return;
            }

            var element = this._getElementFunc( item );

            var group = this.FindGroupForNewItem( item, keyGroups );

            if ( group == null )
            {
                throw new InvalidOperationException();
            }

            group.Add( element );
        }

        private void RemoveItem( TSource removedItem, in IncrementalUpdateToken updateToken )
        {
            var keyGroups = this.ComputeKeyGroups( this.Source.GetValue( this.ObserverToken ) );

            // TODO: process removed groups incrementally
            if ( !this.AreKeysSame( keyGroups ) )
            {
                updateToken.SetBreakingChange( breaksObservers: true );
                return;
            }

            var element = this._getElementFunc( removedItem );

            this._sourceMap[removedItem].Remove( element );
        }

        protected override ReactiveOperatorResult<IEnumerable<IReactiveGroup<TKey, TElement>>> EvaluateFunction( IEnumerable<TSource> source )
        {
            // When re-evaluating, do complete reset.
            // TODO: don't do complete reset

            foreach ( var group in this._groups )
            {
                if ( group.HasObserver )
                {
                    group.Clear();
                }
            }

            // first create a collection of groups along with the source items they came from
            var newGroups = source
                .OrderBy( x => x, this._sourceComparer )
                .GroupAdjacent(
                    s => this._getKeyFunc( s, this.ObserverToken ), sourceItem => (sourceItem, element: this._getElementFunc( sourceItem )), this._equalityComparer )
                .Select( g => (sourceItems: g.Select( x => x.sourceItem ), group: new Group<TKey, TElement>( this, g.Key, g.Select( x => x.element ), this.Version )) )
                .ToImmutableArray();

            // then select just the groups
            this._groups = newGroups.Select( x => x.group ).ToImmutableArray();

            // and a map from source item to group
            this._sourceMap = newGroups.SelectMany( x => x.sourceItems, ( x, sourceItem ) => (x.group, sourceItem) ).ToImmutableDictionary( x => x.sourceItem, x => x.group );

            return new( this._groups );
        }

        private ImmutableArray<IGrouping<TKey, TSource>> ComputeKeyGroups( IEnumerable<TSource> newSource ) =>
            newSource
                .OrderBy( x => x, this._sourceComparer )
                .GroupAdjacent( s => this._getKeyFunc( s, this.ObserverToken ), this._equalityComparer )
                .ToImmutableArray();

        private bool AreKeysSame( ImmutableArray<IGrouping<TKey, TSource>> newGroups )
        {
            var oldGroups = this._groups;

            var newKeys = newGroups.Select( g => g.Key );
            var oldKeys = oldGroups.Select( g => g.Key );
            return newKeys.SequenceEqual( oldKeys, this._equalityComparer );
        }

        private Group<TKey, TElement>? FindGroupForNewItem( TSource newItem, ImmutableArray<IGrouping<TKey, TSource>> newGroups )
        {
            var oldGroups = this._groups;

            foreach ( var (newGroup, oldGroup) in newGroups.Zip( oldGroups, ( ng, og ) => (ng, og) ) )
            {
                if ( newGroup.Contains( newItem ) )
                {
                    return oldGroup;
                }
            }

            return null;
        }

        protected override void OnSourceItemAdded( IReactiveSubscription sourceSubscription, TSource item, in IncrementalUpdateToken updateToken )
        {
            this.AddItem( item, in updateToken );

            updateToken.SetValue( this._groups );
        }

        protected override void OnSourceItemRemoved( IReactiveSubscription sourceSubscription, TSource item, in IncrementalUpdateToken updateToken )
        {
            this.RemoveItem( item, in updateToken );

            updateToken.SetValue( this._groups );
        }

        protected override void OnSourceItemReplaced( IReactiveSubscription sourceSubscription, TSource oldItem, TSource newItem, in IncrementalUpdateToken updateToken )
        {
            this.RemoveItem( oldItem, in updateToken );
            this.AddItem( newItem, in updateToken );

            updateToken.SetValue( this._groups );
        }
    }
}