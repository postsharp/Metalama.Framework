﻿// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Backstage.Diagnostics;

namespace Metalama.Framework.DesignTime.Utilities;

internal class QuietPeriodTimer : IDisposable
{
    private readonly ILogger _logger;
    private readonly List<PauseCookie> _pauseCookies = new();
    private readonly object _sync = new();

    private Timer? _timer;
    private TimeSpan _quietPeriod;
    private DateTime _minimalTickTime;

    public bool IsPending { get; private set; }

    public TimeSpan Delay
    {
        get => this._quietPeriod;
        set => this._quietPeriod = value;
    }

    // ReSharper disable once InconsistentlySynchronizedField
    public bool IsPaused => this._pauseCookies.Count > 0;

    public QuietPeriodTimer( TimeSpan quietPeriod, ILogger logger )
    {
        this._quietPeriod = quietPeriod;
        this._logger = logger;
        this._timer = new Timer( this.OnTick );
    }

    private void OnTick( object? state )
    {
        lock ( this._sync )
        {
            if ( this._timer == null )
            {
                return;
            }

            this._timer.Change( Timeout.Infinite, Timeout.Infinite );

            var now = DateTime.Now;

            if ( now < this._minimalTickTime )
            {
                // It's too early.
                this._logger.Trace?.Log( "Ignoring this tick because it's too early." );

                var newInterval = this._minimalTickTime - now;

                if ( newInterval.TotalMilliseconds > int.MaxValue )
                {
                    newInterval = this._quietPeriod;
                }

                this._timer.Change( (int) newInterval.TotalMilliseconds, Timeout.Infinite );
            }
            else
            {
                this.OnTick();
                this._minimalTickTime = DateTime.Now + this._quietPeriod;
            }
        }
    }

    public void Restart()
    {
        lock ( this._sync )
        {
            if ( this._timer == null )
            {
                return;
            }

            if ( this._pauseCookies.Count == 0 )
            {
                this._minimalTickTime = DateTime.Now + this._quietPeriod;
                this._timer.Change( (int) this._quietPeriod.TotalMilliseconds, Timeout.Infinite );
                this.IsPending = false;
            }

            this.IsPending = true;
        }
    }

    public void ForceNow()
    {
        lock ( this._sync )
        {
            if ( this._timer == null )
            {
                return;
            }

            using ( this.Pause() )
            {
                this.OnTick();
            }
        }
    }

    public PauseCookie Pause()
    {
        lock ( this._sync )
        {
            if ( this._timer == null )
            {
                return default;
            }

            var cookie = new PauseCookie( this );

            this._pauseCookies.Add( cookie );

            // Trace.TraceInformation("QuietPeriodTimer was disabled. disabledLevel={0}", this.pauseCookies.Count);
            this._timer.Change( Timeout.Infinite, Timeout.Infinite );

            return cookie;
        }
    }

    private void Resume( PauseCookie cookie )
    {
        lock ( this._logger )
        {
            if ( this._timer == null )
            {
                return;
            }

            if ( !this._pauseCookies.Remove( cookie ) )
            {
                throw new InvalidOperationException( "Invalid cookie" );
            }

            if ( this._pauseCookies.Count == 0 )
            {
                if ( this.IsPending )
                {
                    this.Restart();
                }
            }
        }
    }

    public void Dispose()
    {
        if ( this._timer != null )
        {
            this._timer.Dispose();
            this._timer = null;
        }
    }

    private void OnTick()
    {
        if ( this._timer == null )
        {
            return;
        }

        this.IsPending = false;

        var handler = this.Tick;
        handler?.Invoke( this, EventArgs.Empty );
    }

    public event EventHandler? Tick;

    public readonly struct PauseCookie : IDisposable
    {
        private readonly QuietPeriodTimer _parent;

        internal PauseCookie( QuietPeriodTimer parent )
        {
            this._parent = parent;
        }

        public void Dispose() => this._parent.Resume( this );
    }
}