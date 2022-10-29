// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Project;
using System;
using System.Threading;

namespace Metalama.Framework.Engine.Utilities.Threading;

public class TestableCancellationTokenSource : IDisposable
{
    public CancellationTokenSource CancellationTokenSource { get; } = new();

    public TestableCancellationToken Token { get; }

    public TestableCancellationTokenSource()
    {
        this.Token = new TestableCancellationToken( this.CancellationTokenSource.Token, this );
    }

    public virtual void OnPossibleCancellationPoint() { }

    public virtual void Dispose()
    {
        this.CancellationTokenSource.Dispose();
    }
}

public interface ITestableCancellationTokenSourceFactory : IService
{
    TestableCancellationTokenSource Create();
}

internal class DefaultTestableCancellationTokenSource : ITestableCancellationTokenSourceFactory
{
    public TestableCancellationTokenSource Create() => new();
}