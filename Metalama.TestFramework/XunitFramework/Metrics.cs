// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System;
using System.Threading;

namespace Metalama.TestFramework.XunitFramework;

internal class Metrics
{
    private readonly Metrics? _parent;

    private int _testsRun;
    private int _testFailed;
    private int _testSkipped;
    private int _testsRemaining;
    private int _testsStarted;
    private long _executionTime;

    public Metrics( Metrics? parent = null )
    {
        this._parent = parent;
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
            this.Finished?.Invoke();
        }
    }

    public void OnTestStarted()
    {
        if ( Interlocked.Increment( ref this._testsStarted ) == 1 )
        {
            this.Started?.Invoke();
        }

        this._parent?.OnTestStarted();
    }

    private void OnTestFinished()
    {
        if ( Interlocked.Decrement( ref this._testsRemaining ) == 0 )
        {
            this.Finished?.Invoke();
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