namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Gives access to configuration options (typically values pulled from MSBuild). The
    /// typical implementation is <see cref="AnalyzerBuildOptionsSource"/>, but other implementations can be used for testing.
    /// </summary>
    public interface IBuildOptionsSource
    {
        /// <summary>
        /// Gets a configuration value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue( string name, out string? value );
    }

    public interface IBuildOptions
    {
        bool AttachDebugger { get; }
        bool MapPdbToTransformedCode { get; }

        /// <summary>
        /// Gets the directory in which the code for the compile-time assembly should be stored, or a null or empty
        /// string to mean that the generated code should not be stored.
        /// </summary>
        string? CompileTimeProjectDirectory { get; }
    }

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