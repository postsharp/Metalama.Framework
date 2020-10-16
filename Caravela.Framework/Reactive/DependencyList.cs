using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Xml;

namespace Caravela.Reactive
{
    struct DependencyList
    {
        private volatile Dictionary<IReactiveSource, Dependency> _dependencies;
        private readonly IReactiveObserver _parent;

        public DependencyList(IReactiveObserver parent)
        {
            this._parent = parent;
            this._dependencies = null;
        }

        public bool IsEmpty => this._dependencies == null || this._dependencies.Count == 0;


        public void Add<T>(T source)
            where T : IReactiveObservable<IReactiveObserver>, IReactiveSource
        {
            if (source == null)
            {
                return;
            }

#if DEBUG
            if (this._parent == null)
                throw new InvalidOperationException();
#endif


            if (this._dependencies == null)
            {
                Interlocked.CompareExchange(ref this._dependencies,
                    new Dictionary<IReactiveSource, Dependency>(),
                    null);
            }

            lock (this._dependencies)
            {
                if (!this._dependencies.ContainsKey(source))
                {
                    var subscription = source.AddObserver(this._parent);
                    Debug.Assert(subscription != null);
                    this._dependencies.Add(source, new Dependency(subscription, source.Version));
                }
            }
        }

        public void Clear()
        {
            // Dispose previous dependencies.
            var dependencies = this._dependencies;

            if (dependencies != null)
            {
                lock (dependencies)
                {
                    foreach (var subscription in dependencies.Values)
                    {
                        subscription.Subscription.Dispose();
                    }

                    dependencies.Clear();
                }
            }
        }

        public bool IsDirty()
        {
            if (this._dependencies == null)
                return false;

            foreach (var dependency in _dependencies)
            {
                if (dependency.Key.Version > dependency.Value.Version)
                {
                    return true;
                }
            }

            return false;
        }

        readonly struct Dependency
        {
            public IReactiveSubscription Subscription { get; }
            public int Version { get;  }

            public Dependency(IReactiveSubscription subscription, int version)
            {
                this.Subscription = subscription;
                this.Version = version;
            }
        }
    }
}