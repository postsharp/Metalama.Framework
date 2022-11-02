// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Threading;

namespace Metalama.Framework.Engine.Utilities.Threading;

public readonly struct TestableCancellationToken
{
    private readonly CancellationToken _cancellationToken;

#if DEBUG
    private readonly TestableCancellationTokenSource? _testableSource;
#endif

    public static TestableCancellationToken None => default;

    public void ThrowIfCancellationRequested()
    {
#if DEBUG
        this._testableSource?.OnPossibleCancellationPoint();
#endif

        this._cancellationToken.ThrowIfCancellationRequested();
    }

    public static implicit operator CancellationToken( TestableCancellationToken token )
    {
#if DEBUG
        token._testableSource?.OnPossibleCancellationPoint();
#endif

        return token._cancellationToken;
    }

    internal TestableCancellationToken( CancellationToken cancellationToken, TestableCancellationTokenSource? testableSource = null )
    {
        this._cancellationToken = cancellationToken;

#if DEBUG
        this._testableSource = testableSource;
#endif
    }
}