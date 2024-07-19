// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// define TRACE_LEAKS to get additional diagnostics that can lead to the leak sources. note: it will
// make everything about 2-3x slower
// 
// #define TRACE_LEAKS

// define DETECT_LEAKS to detect possible leaks
// #if DEBUG
// #define DETECT_LEAKS  //for now always enable DETECT_LEAKS in debug.
// #endif

using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.Threading;
#if DETECT_LEAKS
using System.Runtime.CompilerServices;
#endif

namespace Metalama.Framework.Engine.Utilities.Caching;

/// <summary>
/// Generic implementation of object pooling pattern with predefined pool size limit. The main
/// purpose is that limited number of frequently used objects can be kept in the pool for
/// further recycling.
/// 
/// Notes: 
/// 1) it is not the goal to keep all returned objects. Pool is not meant for storage. If there
///    is no space in the pool, extra returned objects will be dropped.
/// 
/// 2) it is implied that if object was obtained from a pool, the caller will return it back in
///    a relatively short time. Keeping checked out objects for long durations is ok, but 
///    reduces usefulness of pooling. Just new up your own.
/// 
/// Not returning objects to the pool in not detrimental to the pool's work, but is a bad practice. 
/// Rationale: 
///    If there is no intent for reusing the object, do not use pool - just use "new". 
/// </summary>
[PublicAPI]
public class ObjectPool<T>
    where T : class
{
    // ReSharper disable once UseNameofExpressionForPartOfTheString
    [DebuggerDisplay( "{Value,nq}" )]
    private struct Element
    {
        internal T? Value;
    }

    private readonly Element[] _items;

    // factory is stored for the lifetime of the pool. We will call this only when pool needs to
    // expand. compared to "new T()", Func gives more flexibility to implementers and faster
    // than "new T()".
    private readonly Func<T> _factory;

    // Storage for the pool objects. The first item is stored in a dedicated field because we
    // expect to be able to satisfy most requests from it.
    private T? _firstItem;

    public bool TrimOnFree { get; }

#if DETECT_LEAKS
        private static readonly ConditionalWeakTable<T, LeakTracker> leakTrackers = new ConditionalWeakTable<T, LeakTracker>();

        private class LeakTracker : IDisposable
        {
            private volatile bool disposed;

#if TRACE_LEAKS
            internal volatile object Trace = null;
#endif

            public void Dispose()
            {
                disposed = true;
                GC.SuppressFinalize(this);
            }

            private string GetTrace()
            {
#if TRACE_LEAKS
                return Trace == null ? "" : Trace.ToString();
#else
                return "Leak tracing information is disabled. Define TRACE_LEAKS on ObjectPool`1.cs to get more info \n";
#endif
            }

            ~LeakTracker()
            {
                if (!this.disposed && !Environment.HasShutdownStarted)
                {
                    var trace = GetTrace();

                    // If you are seeing this message it means that object has been allocated from the pool 
                    // and has not been returned back. This is not critical, but turns pool into rather 
                    // inefficient kind of "new".
                    Debug.WriteLine($"TRACEOBJECTPOOLLEAKS_BEGIN\nPool detected potential leaking of {typeof(T)}. \n Location of the leak: \n {GetTrace()} TRACEOBJECTPOOLLEAKS_END");
                }
            }
        }
#endif

    internal ObjectPool( Func<T> factory, bool trimOnFree = true )
        : this( factory, Environment.ProcessorCount * 2, trimOnFree ) { }

    internal ObjectPool( Func<T> factory, int size, bool trimOnFree = true )
    {
        this._factory = factory;
        this._items = new Element[size - 1];
        this.TrimOnFree = trimOnFree;
    }

    internal ObjectPool( Func<ObjectPool<T>, T> factory, int size )
    {
        this._factory = () => factory( this );
        this._items = new Element[size - 1];
    }

    protected virtual void CleanUp( T obj ) { }

    protected virtual void Recycle( T obj ) { }

    private T CreateInstance() => this._factory();

    /// <summary>
    /// Produces an instance.
    /// </summary>
    /// <remarks>
    /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
    /// Note that Free will try to store recycled objects close to the start thus statistically 
    /// reducing how far we will typically search.
    /// </remarks>
    public ObjectPoolHandle<T> Allocate()
    {
        // PERF: Examine the first element. If that fails, AllocateSlow will look at the remaining elements.
        // Note that the initial read is optimistically not synchronized. That is intentional. 
        // We will interlock only when we have a candidate. in a worst case we may miss some
        // recently returned objects. Not a big deal.
        var inst = this._firstItem;

        if ( inst == null || inst != Interlocked.CompareExchange( ref this._firstItem, null, inst ) )
        {
            inst = this.AllocateSlow();
        }
        else
        {
            this.Recycle( inst );
        }

#if DETECT_LEAKS
            var tracker = new LeakTracker();
            leakTrackers.Add(inst, tracker);

#if TRACE_LEAKS
            var frame = CaptureStackTrace();
            tracker.Trace = frame;
#endif
#endif
        return new ObjectPoolHandle<T>( this, inst );
    }

    private T AllocateSlow()
    {
        var items = this._items;

        for ( var i = 0; i < items.Length; i++ )
        {
            // Note that the initial read is optimistically not synchronized. That is intentional. 
            // We will interlock only when we have a candidate. in a worst case we may miss some
            // recently returned objects. Not a big deal.
            var inst = items[i].Value;

            if ( inst != null )
            {
                if ( inst == Interlocked.CompareExchange( ref items[i].Value, null, inst ) )
                {
                    this.Recycle( inst );

                    return inst;
                }
            }
        }

        return this.CreateInstance();
    }

    /// <summary>
    /// Returns objects to the pool.
    /// </summary>
    /// <remarks>
    /// Search strategy is a simple linear probing which is chosen for it cache-friendliness.
    /// Note that Free will try to store recycled objects close to the start thus statistically 
    /// reducing how far we will typically search in Allocate.
    /// </remarks>
    public void Free( T obj )
    {
        this.CleanUp( obj );

        this.Validate( obj );
        this.ForgetTrackedObject( obj );

        if ( this._firstItem == null )
        {
            // Intentionally not using interlocked here. 
            // In a worst case scenario two objects may be stored into same slot.
            // It is very unlikely to happen and will only mean that one of the objects will get collected.
            this._firstItem = obj;
        }
        else
        {
            this.FreeSlow( obj );
        }
    }

    private void FreeSlow( T obj )
    {
        var items = this._items;

        for ( var i = 0; i < items.Length; i++ )
        {
            if ( items[i].Value == null )
            {
                // Intentionally not using interlocked here. 
                // In a worst case scenario two objects may be stored into same slot.
                // It is very unlikely to happen and will only mean that one of the objects will get collected.
                items[i].Value = obj;

                break;
            }
        }
    }

    /// <summary>
    /// Removes an object from leak tracking.  
    /// 
    /// This is called when an object is returned to the pool.  It may also be explicitly 
    /// called if an object allocated from the pool is intentionally not being returned
    /// to the pool.  This can be of use with pooled arrays if the consumer wants to 
    /// return a larger array to the pool than was originally allocated.
    /// </summary>
    [Conditional( "DEBUG" )]
#pragma warning disable CA1822
    internal void ForgetTrackedObject( T old, T? replacement = null )
#pragma warning restore CA1822
    {
#if DETECT_LEAKS
            LeakTracker tracker;
            if (leakTrackers.TryGetValue(old, out tracker))
            {
                tracker.Dispose();
                leakTrackers.Remove(old);
            }
            else
            {
                var trace = CaptureStackTrace();
                Debug.WriteLine($"TRACEOBJECTPOOLLEAKS_BEGIN\nObject of type {typeof(T)} was freed, but was not from pool. \n Callstack: \n {trace} TRACEOBJECTPOOLLEAKS_END");
            }

            if (replacement != null)
            {
                tracker = new LeakTracker();
                leakTrackers.Add(replacement, tracker);
            }
#endif
    }

#if DETECT_LEAKS
        private static Lazy<Type> _stackTraceType = new Lazy<Type>(() => Type.GetType("System.Diagnostics.StackTrace"));

        private static object CaptureStackTrace()
        {
            return Activator.CreateInstance(_stackTraceType.Value);
        }
#endif

    [Conditional( "DEBUG" )]
    private void Validate( object obj )
    {
        Debug.Assert( obj != null, "freeing null?" );

        Debug.Assert( this._firstItem != obj, "freeing twice?" );

        var items = this._items;

        for ( var i = 0; i < items.Length; i++ )
        {
            var value = items[i].Value;

            if ( value == null )
            {
                return;
            }

            Debug.Assert( value != obj, "freeing twice?" );
        }
    }
}