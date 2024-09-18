// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Code;
using System.Collections.Generic;

namespace Metalama.Framework.Engine.CodeModel.References;

internal class RefEqualityComparer<T> : IEqualityComparer<IRef<T>> where T : class, ICompilationElement
{
    public static RefEqualityComparer<T> Default { get; } = new();

    private RefEqualityComparer() { }

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

        return x.Equals( y );
    }

    public int GetHashCode( IRef<T> obj )
    {
        return obj.GetHashCode();
    }
}