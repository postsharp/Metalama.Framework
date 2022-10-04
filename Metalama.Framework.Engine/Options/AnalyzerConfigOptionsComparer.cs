// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Options;

internal class AnalyzerConfigOptionsComparer : IEqualityComparer<AnalyzerConfigOptions>
{
    private readonly ImmutableArray<string> _allProperties;

    public AnalyzerConfigOptionsComparer( IEnumerable<string> msbuildProperties )
    {
        this._allProperties = msbuildProperties.Select( n => $"build_property.{n}" ).ToImmutableArray();
    }

    public bool Equals( AnalyzerConfigOptions? x, AnalyzerConfigOptions? y )
    {
        if ( ReferenceEquals( x, y ) )
        {
            return true;
        }

        if ( x == null || y == null )
        {
            return false;
        }

        foreach ( var propertyName in this._allProperties )
        {
            if ( x.TryGetValue( propertyName, out var xProperty ) != y.TryGetValue( propertyName, out var yProperty ) )
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

    public int GetHashCode( AnalyzerConfigOptions obj )
    {
        HashCode hashCode = default;

        foreach ( var propertyName in this._allProperties )
        {
            if ( obj.TryGetValue( propertyName, out var propertyValue ) )
            {
                hashCode.Add( propertyValue, StringComparer.Ordinal );
            }
        }

        return hashCode.ToHashCode();
    }
}