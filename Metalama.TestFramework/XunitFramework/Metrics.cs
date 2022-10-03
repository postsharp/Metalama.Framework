﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Threading;

namespace Metalama.TestFramework.XunitFramework;

internal class Metrics
{
    private readonly Metrics? _parent;
    private readonly object _eventLock;

    private int _testsRun;
    private int _testFailed;
    private int _testSkipped;
    private int _testsRemaining;
    private int _testsStarted;
    private long _executionTime;

    public Metrics( Metrics parent )
    {
        this._parent = parent;
        this._eventLock = parent._eventLock;
    }

    public Metrics( object eventLock )
    {
        this._eventLock = eventLock;
    }

    public int TestsRun => this._testsRun;

    public int TestFailed => this._testFailed;

    public int TestSkipped => this._testSkipped;

    public decimal ExecutionTime => (decimal) TimeSpan.FromMilliseconds( this._executionTime ).TotalSeconds;

    public event Action? Started;

    public event Action? Finished;

    public void OnTestsDiscovered( int count )
    {
        Interlocked.Add( ref this._testsRemaining, count );
        this._parent?.OnTestsDiscovered( count );

        if ( count == 0 )
        {
            lock ( this._eventLock )
            {
                this.Finished?.Invoke();
            }
        }
    }

    public void OnTestStarted()
    {
        if ( Interlocked.Increment( ref this._testsStarted ) == 1 )
        {
            lock ( this._eventLock )
            {
                this.Started?.Invoke();
            }
        }

        this._parent?.OnTestStarted();
    }

    private void OnTestFinished()
    {
        if ( Interlocked.Decrement( ref this._testsRemaining ) == 0 )
        {
            lock ( this._eventLock )
            {
                this.Finished?.Invoke();
            }
        }

        this._parent?.OnTestFinished();
    }

    public void OnTestSucceeded( TimeSpan duration )
    {
        Interlocked.Increment( ref this._testsRun );
        Interlocked.Add( ref this._executionTime, (long) duration.TotalMilliseconds );
        this._parent?.OnTestSucceeded( duration );
        this.OnTestFinished();
    }

    public void OnTestFailed( TimeSpan duration )
    {
        Interlocked.Increment( ref this._testFailed );
        Interlocked.Add( ref this._executionTime, (long) duration.TotalMilliseconds );
        this._parent?.OnTestFailed( duration );
        this.OnTestFinished();
    }

    public void OnTestSkipped()
    {
        Interlocked.Increment( ref this._testSkipped );
        this._parent?.OnTestSkipped();
        this.OnTestFinished();
    }
}