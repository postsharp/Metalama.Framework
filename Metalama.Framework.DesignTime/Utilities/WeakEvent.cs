﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Runtime.CompilerServices;

namespace Metalama.Framework.DesignTime.Utilities;

#pragma warning disable SA1402 // File may only contain a single type

// Based on PostSharp.Patterns.Model.WeakEventHandler, but significantly changed.
public sealed class WeakEvent<TEventArgs> : WeakEventBase<Action<TEventArgs>, TEventArgs>
{
    public void Invoke( TEventArgs eventArgs )
    {
        // Take a snapshot of the targets list.
        var invocationList = this.Targets;

        var needCleanUp = false;

        if ( invocationList == null )
        {
            return;
        }

        foreach ( var obj in invocationList )
        {
            if ( obj == null )
            {
                continue;
            }

            if ( !obj.TryGetTarget( out var target ) )
            {
                needCleanUp = true;
            }
            else
            {
                if ( this.GetHandler( target ) is { } handler )
                {
                    handler.Invoke( eventArgs );
                }
            }
        }

        if ( needCleanUp )
        {
            this.CleanUp();
        }
    }
}

public sealed class AsyncWeakEvent<TEventArgs> : WeakEventBase<Func<TEventArgs, Task>, TEventArgs>
{
    public async Task InvokeAsync( TEventArgs eventArgs )
    {
        // Take a snapshot of the targets list.
        var invocationList = this.Targets;

        var needCleanUp = false;

        if ( invocationList == null )
        {
            return;
        }

        foreach ( var obj in invocationList )
        {
            if ( obj == null )
            {
                continue;
            }

            if ( !obj.TryGetTarget( out var target ) )
            {
                needCleanUp = true;
            }
            else
            {
                if ( this.GetHandler( target ) is { } handler )
                {
                    await handler.Invoke( eventArgs );
                }
            }
        }

        if ( needCleanUp )
        {
            this.CleanUp();
        }
    }
}

public abstract class WeakEventBase<TDelegate, TEventArgs>
    where TDelegate : MulticastDelegate
{
    private readonly object _lock;

    private readonly ConditionalWeakTable<object, TDelegate> _handlers = new();

    protected WeakReference<object>?[]? Targets { get; private set; }

    public bool HasHandlers()
    {
        var targets = this.Targets;

        if ( targets == null )
        {
            return false;
        }

        for ( var i = 0; i < targets.Length; i++ )
        {
            if ( targets[i] != null )
            {
                return true;
            }
        }

        return false;
    }

    public WeakEventBase()
    {
        this._lock = new();
    }

    public void AddHandler( TDelegate handler )
    {
        if ( handler.Target == null )
        {
            throw new ArgumentException( "Delegates with no target are not supported." );
        }

        lock ( this._lock )
        {
            // Take a local copy of the array to keep the shared copy consistent for readers.
            var myTargets = this.Targets;

            int index;

            if ( myTargets == null )
            {
                myTargets = new WeakReference<object>[1];
                index = 0;
            }
            else
            {
                index = -1;
                for ( var i = 0; i < myTargets.Length; i++ )
                {
                    var handlerRef = myTargets[i];
                    if ( handlerRef == null )
                    {
                        index = i;
                        // We continue to loop because we want to keep cleaning the list.
                    }
                    else if ( !handlerRef.TryGetTarget( out _ ) )
                    {
                        myTargets[i] = null;
                    }
                }

                if ( index < 0 )
                {
                    index = myTargets.Length;
                    Array.Resize( ref myTargets, myTargets.Length * 2 );
                }
            }

            this._handlers.Add( handler.Target, handler );

            myTargets[index] = new( handler.Target );

            this.Targets = myTargets;
        }
    }

    public void RemoveHandler( TDelegate handler )
    {
        lock ( this._lock )
        {
            // Take a local copy of the array to keep the shared copy consistent for readers.
            var myTargets = this.Targets;

            if ( myTargets == null )
            {
                return;
            }

            for ( var i = myTargets.Length - 1; i >= 0; i-- )
            {
                var targetRef = myTargets[i];
                if ( targetRef != null )
                {
                    if ( !targetRef.TryGetTarget( out var t ) )
                    {
                        myTargets[i] = null;
                    }
                    else if ( t == handler.Target )
                    {
                        this._handlers.Remove( handler.Target );

                        myTargets[i] = null;

                        return;
                    }
                }
            }
        }
    }

    protected void CleanUp()
    {
        lock ( this._lock )
        {
            var myTargets = this.Targets;

            if ( myTargets == null )
            {
                return;
            }

            for ( var i = 0; i < myTargets.Length; i++ )
            {
                var handlerRef = myTargets[i];
                if ( handlerRef != null )
                {
                    if ( !handlerRef.TryGetTarget( out _ ) )
                    {
                        myTargets[i] = null;
                    }
                }
            }
        }
    }

    protected TDelegate? GetHandler( object target )
    {
        this._handlers.TryGetValue( target, out var handler );

        return handler;
    }

    public Accessors GetAccessors() => new( this );

    public readonly struct Accessors
    {
        private readonly WeakEventBase<TDelegate, TEventArgs> _parent;

        public Accessors( WeakEventBase<TDelegate, TEventArgs> parent )
        {
            this._parent = parent;
        }

        public void RegisterHandler( TDelegate handler ) => this._parent.AddHandler( handler );

        public void UnregisterHandler( TDelegate handler ) => this._parent.RemoveHandler( handler );
    }
}