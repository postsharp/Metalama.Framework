// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using System;
using System.Linq;

namespace Metalama.Framework.Engine.Options;

public static class ProjectOptionsEqualityComparer
{
    public static bool Equals( IProjectOptions x, IProjectOptions y )
    {
        if ( ReferenceEquals( x, y ) )
        {
            return true;
        }

        var propertyNames = x.PropertyNames.Union( y.PropertyNames );

        foreach ( var propertyName in propertyNames )
        {
            if ( x.TryGetProperty( propertyName, out var xProperty ) != y.TryGetProperty( propertyName, out var yProperty ) )
            {
                return false;
            }
            else if ( !StringComparer.Ordinal.Equals( xProperty, yProperty ) )
            {
                return false;
            }
        }

        return true;
    }
}