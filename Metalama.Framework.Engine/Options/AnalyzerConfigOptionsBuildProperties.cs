// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Microsoft.CodeAnalysis.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Engine.Options;

internal static class AnalyzerConfigOptionsBuildProperties
{
    private const string _prefix = "build_property.";

    public static string ToAnalyzerConfigName( string msBuildPropertyName ) => _prefix + msBuildPropertyName;

    public static string ToMsBuildPropertyName( string analyzerConfigName )
    {
        Invariant.Assert( analyzerConfigName.StartsWith( _prefix, StringComparison.Ordinal ) );

        return analyzerConfigName.Substring( _prefix.Length );
    }

    public static IEnumerable<string> GetBuildProperties( this AnalyzerConfigOptions options )
    {
#if ROSLYN_4_4_0_OR_GREATER
        return options.Keys.Where( name => name.StartsWith( _prefix, StringComparison.Ordinal ) );
#else
        IEnumerable<string> msBuildProperties;

        if ( options.TryGetValue( ToAnalyzerConfigName( MSBuildPropertyNames.MetalamaCompilerVisiblePropertiesList ), out var propertiesList ) )
        {
            msBuildProperties = propertiesList.Split( ',' );
        }
        else
        {
            // This shouldn't happen, but if it somehow does, I think it's better to have worse desing-time experience for custom compile-time properties, rather than failing.
            msBuildProperties = MSBuildPropertyNames.All;
        }

        return msBuildProperties.Select( ToAnalyzerConfigName );
#endif
    }
}