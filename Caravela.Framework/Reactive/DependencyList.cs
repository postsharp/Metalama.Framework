using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Caravela.Reactive
{
    struct DependencyList 
    {
        private volatile Dictionary<object, IReactiveSubscription>? _dependencies;
        private readonly IReactiveObserver _parent;

        public DependencyList(IReactiveObserver parent)
        {
            this._parent = parent;
            this._dependencies = null;
        }

        public bool IsEmpty => this._dependencies == null || this._dependencies.Count == 0;
        

        public void Add(IReactiveObservable<IReactiveObserver> observable)
        {
            #if DEBUG
            if ( this._parent == null )
                throw new InvalidOperationException();
            #endif
            
            
            if (this._dependencies == null)
            {
                Interlocked.CompareExchange(ref this._dependencies,
                    new Dictionary<object, IReactiveSubscription>(),
                    null);
            }

            lock (this._dependencies)
            {
                if (!this._dependencies.ContainsKey(observable))
                {
                    var subscription = observable.AddObserver(this._parent);
                    Debug.Assert(subscription != null);
                    this._dependencies.Add(observable, subscription!);
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
                        subscription.Dispose();
                    }
                    
                    dependencies.Clear();
                }
            }
        }


    }
}