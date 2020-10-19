using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml;

namespace Caravela.Reactive
{
    /// <summary>
    /// Implementation of <see cref="IReactiveTokenCollector"/>. This is a mutable struct! Don't make it a property
    /// or a read-only field.
    /// </summary>
    internal struct DependencyList
    {
        private volatile Node _dependencies;
        private readonly IReactiveObserver _parent;

        /// <summary>
        /// Creates a new <see cref="DependencyList"/>.
        /// </summary>
        /// <param name="parent">The implementation of <see cref="IReactiveObserver"/>,
        /// which should receive change notifications.
        /// </param>
        public DependencyList(IReactiveObserver parent)
        {
            this._parent = parent;
            this._dependencies = null;
        }

        public bool IsEmpty => this._dependencies == null;

        private Node FindNode(IReactiveObservable<IReactiveObserver> source)
        {
            for (var node = this._dependencies; node != null; node = node.Next)
            {
                if (node.Source == source)
                {
                    return node;
                }
            }

            return null;
        }


        /// <summary>
        /// Adds a dependency to the list and starts observing changes.
        /// </summary>
        /// <param name="source">The dependent source.</param>
        /// <param name="version">The version of the source when it was evaluated.</param>
        /// <typeparam name="T">An <see cref="IReactiveObservable{T}"/>.</typeparam>
        /// <exception cref="InvalidOperationException"></exception>
        public void Add<T>(T source, int version)
            where T : IReactiveObservable<IReactiveObserver>
        {
            if (source == null)
            {
                return;
            }

#if DEBUG
            if (this._parent == null)
                throw new InvalidOperationException();
#endif

            // Try to find an existing node.
            var existingNode = this.FindNode(source);
            if (existingNode != null)
            {
                // Not sure what to do if the version if different. 
                Debug.Assert(existingNode.Version == version);
                return;
            }
            
            // No existing node. Add an observer and create a new node.
            var subscription = source.AddObserver(this._parent);
            var node = new Node(source, subscription, version);

            while (true)
            {
                var head = this._dependencies;
                node.Next = head;

                if (Interlocked.CompareExchange(ref this._dependencies, node, head) == head)
                {
                    // Success.
                    return;
                }
            }


        }

        /// <summary>
        /// Disposes all subscriptions to dependencies and clears the list.
        /// </summary>
        public void Clear()
        {
            for (var node = this._dependencies; node != null; node = node.Next)
            {
                node.Subscription.Dispose();
            }

            this._dependencies = null;
        }

        
        /// <summary>
        /// Determines if any dependency is out of date, based on the cached version number
        /// and the current version number of the dependent source.
        /// </summary>
        /// <returns></returns>
        public bool IsDirty()
        {
            if (this._dependencies == null)
                return false;

            // No lock is necessary for reading.
            for (var node = this._dependencies; node != null; node = node.Next)
            {
                if (node.Version > node.Source.Version)
                {
                    return true;
                }
            }

            return false;
        }

        private class Node
        {
            public IReactiveObservable<IReactiveObserver> Source { get; }
            public IReactiveSubscription Subscription { get; }
            public int Version { get;  }

            public Node Next { get; set; }

            public Node(IReactiveObservable<IReactiveObserver> source, IReactiveSubscription subscription, int version)
            {
                this.Source = source;
                this.Subscription = subscription;
                this.Version = version;
            }
        }
    }
}