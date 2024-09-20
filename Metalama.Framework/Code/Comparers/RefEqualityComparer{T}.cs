﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System.Collections.Generic;

namespace Metalama.Framework.Code.Comparers;

public sealed class RefEqualityComparer<T> : IEqualityComparer<IRef<T>?>, IRefEqualityComparer
    where T : class, ICompilationElement
{
    private readonly RefComparison _comparison;

    public static RefEqualityComparer<T> Default { get; } = new( RefComparison.Default );

    public static RefEqualityComparer<T> IncludeNullability { get; } = new( RefComparison.IncludeNullability );

    public static RefEqualityComparer<T> Structural { get; } = new( RefComparison.Structural );

    public static RefEqualityComparer<T> StructuralIncludeNullability { get; } =
        new( RefComparison.Structural | RefComparison.IncludeNullability );

    private RefEqualityComparer( RefComparison comparison )
    {
        this._comparison = comparison;
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

        return x.Equals( y, this._comparison );
    }

    public int GetHashCode( IRef<T> obj ) => obj.GetHashCode( this._comparison );
}