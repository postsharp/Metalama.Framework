// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Utilities;
using System;
using System.Collections.Generic;

namespace Metalama.Tool.Licensing;

internal sealed class PackageVersionComparer : Comparer<string>
{
    private static Version GetVersion( string str )
    {
        var indexOfDash = str.IndexOfOrdinal( '-' );

        if ( indexOfDash < 0 )
        {
            return new Version( str );
        }
        else
        {
            return new Version( str.Substring( 0, indexOfDash ) );
        }
    }

    public override int Compare( string? x, string? y )
    {
        if ( x == null || y == null )
        {
            throw new ArgumentNullException();
        }

        var compareVersion = GetVersion( x ).CompareTo( GetVersion( y ) );

        if ( compareVersion != 0 )
        {
            return compareVersion;
        }

        // Compare by string.
        return StringComparer.Ordinal.Compare( x, y );
    }
}