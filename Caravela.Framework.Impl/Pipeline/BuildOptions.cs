// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Default implementation of <see cref="IBuildOptions"/>, based on a <see cref="IBuildOptionsSource"/>
    /// reading options passed by MSBuild.
    /// </summary>
    public class BuildOptions : IBuildOptions
    {
        private readonly IBuildOptionsSource _source;

        public BuildOptions( IBuildOptionsSource source )
        {
            this._source = source;
        }

        public bool AttachDebugger => this.GetBooleanOption( "DebugCaravela" );

        public bool MapPdbToTransformedCode => this.GetBooleanOption( "CaravelaDebugTransformedCode" );

        public string? CompileTimeProjectDirectory => this.GetStringOption( "CaravelaCompileTimeProjectDirectory" );

        public string? CrashReportDirectory => this.GetStringOption( "CaravelaCrashReportDirectory" );

        private bool GetBooleanOption( string name, bool defaultValue = false )
        {
            if ( this._source.TryGetValue( $"build_property.{name}", out var flagString ) && bool.TryParse( flagString, out var flagValue ) )
            {
                return flagValue;
            }

            return defaultValue;
        }

        private string? GetStringOption( string name, string? defaultValue = null )
        {
            if ( this._source.TryGetValue( $"build_property.{name}", out var flagString ) )
            {
                return flagString;
            }

            return defaultValue;
        }
    }
}