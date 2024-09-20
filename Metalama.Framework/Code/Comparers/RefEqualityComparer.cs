// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Code.Comparers;

public sealed class RefEqualityComparer : IEqualityComparer<IRef>, IRefEqualityComparer
{
    public static RefEqualityComparer Default { get; } = new();

    private RefEqualityComparer() { }

    public bool Equals( IRef? x, IRef? y )
    {
        if ( ReferenceEquals( x, y ) )
        {
            return true;
        }

        if ( x == null || y == null )
        {
            return false;
        }

        return x.Equals( y );
    }

    public int GetHashCode( IRef obj ) => obj.GetHashCode( RefComparison.Default );
}