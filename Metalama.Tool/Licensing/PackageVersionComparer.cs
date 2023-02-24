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

        string versionString;

        if ( indexOfDash < 0 )
        {
            versionString = str;
        }
        else
        {
            versionString = str.Substring( 0, indexOfDash );
        }

        if ( Version.TryParse( versionString, out var version ) )
        {
            return version;
        }
        else
        {
            return new Version( 0, 0 );
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