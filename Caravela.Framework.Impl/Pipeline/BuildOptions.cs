using System.Diagnostics;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Default implementation of <see cref="IBuildOptions"/>, based on a <see cref="IBuildOptionsSource"/>
    /// reading options passed by MSBuild.
    /// </summary>
    public class BuildOptions : IBuildOptions
    {
        private IBuildOptionsSource _source;

        public BuildOptions( IBuildOptionsSource source )
        {
            this._source = source;
        }

        public bool AttachDebugger => this.GetBooleanOption( "DebugCaravela" );

        public bool MapPdbToTransformedCode => this.GetBooleanOption( "CaravelaDebugTransformedCode" );

        public string? CompileTimeProjectDirectory => this.GetStringOption( "CaravelaCompileTimeProjectDirectory" );

        public string CrashReportDirectory => this.GetStringOption( "CaravelaCrashReportDirectory" );

        public bool WriteUnhandledExceptionsToFile => true;

        private bool GetBooleanOption( string name, bool defaultValue = false )
        {
            if ( this._source.TryGetValue( $"build_property.{name}", out var flagString ) && bool.TryParse( flagString, out var flagValue ) )
            {
                return flagValue;
            }
            else
            {
                return defaultValue;
            }
        }

        private string? GetStringOption( string name, string defaultValue = null )
        {
            if ( this._source.TryGetValue( $"build_property.{name}", out var flagString ) )
            {
                return flagString;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}