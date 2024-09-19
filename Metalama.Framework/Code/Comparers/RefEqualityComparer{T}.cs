// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Code.Comparers;

public sealed class RefEqualityComparer<T> : IEqualityComparer<IRef<T>?>
    where T : class, ICompilationElement
{
    private readonly RefComparisonOptions _comparisonOptions;

    public static RefEqualityComparer<T> Default { get; } = new( RefComparisonOptions.Default );

    public static RefEqualityComparer<T> IncludeNullability { get; } = new( RefComparisonOptions.IncludeNullability );

    public static RefEqualityComparer<T> Structural { get; } = new( RefComparisonOptions.Structural );

    public static RefEqualityComparer<T> StructuralIncludeNullability { get; } =
        new( RefComparisonOptions.Structural | RefComparisonOptions.IncludeNullability );

    private RefEqualityComparer( RefComparisonOptions comparisonOptions )
    {
        this._comparisonOptions = comparisonOptions;
    }

    public bool Equals( IRef<T>? x, IRef<T>? y )
    {
        if ( ReferenceEquals( x, y ) )
        {
            return true;
        }

        if ( x == null || y == null )
        {
            return false;
        }

        return x.Equals( y, this._comparisonOptions );
    }

    public int GetHashCode( IRef<T> obj ) => obj.GetHashCode( this._comparisonOptions );
}

