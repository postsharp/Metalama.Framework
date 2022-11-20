// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Engine.Utilities.Comparers;

public class ReverseComparer<T> : IComparer<T>
{
    private readonly IComparer<T> _underlying;

    public static IComparer<T> Default { get; } = new ReverseComparer<T>( Comparer<T>.Default );

    public ReverseComparer( IComparer<T> underlying ) 
    {
        this._underlying = underlying;
    }

    public int Compare( T? x, T? y ) => -this._underlying.Compare( x, y );
}