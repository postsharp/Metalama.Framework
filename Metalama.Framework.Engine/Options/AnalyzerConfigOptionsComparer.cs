// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Metalama.Framework.Engine.Options;

internal sealed class AnalyzerConfigOptionsComparer : IEqualityComparer<AnalyzerConfigOptions>
{
    private readonly ImmutableArray<string>? _allProperties;

    public AnalyzerConfigOptionsComparer( IEnumerable<string>? msbuildProperties )
    {
#if !ROSLYN_4_4_0_OR_GREATER
        msbuildProperties ??= MSBuildPropertyNames.All;
#endif
        this._allProperties = msbuildProperties?.Select( n => $"build_property.{n}" ).ToImmutableArray();
    }

    private static bool IsBuildProperty( string propertyName ) => propertyName.StartsWith( "build_property.", StringComparison.Ordinal );

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

#if ROSLYN_4_4_0_OR_GREATER
        var allProperties = this._allProperties ?? x.Keys.Union( y.Keys ).Where( IsBuildProperty );
#else
        var allProperties = this._allProperties.Value;
#endif

        foreach ( var propertyName in allProperties )
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

#if ROSLYN_4_4_0_OR_GREATER
        var allProperties = this._allProperties ?? obj.Keys.Where( IsBuildProperty );
#else
        var allProperties = this._allProperties.Value;
#endif

        foreach ( var propertyName in allProperties )
        {
            if ( obj.TryGetValue( propertyName, out var propertyValue ) )
            {
                hashCode.Add( propertyValue, StringComparer.Ordinal );
            }
        }

        return hashCode.ToHashCode();
    }
}