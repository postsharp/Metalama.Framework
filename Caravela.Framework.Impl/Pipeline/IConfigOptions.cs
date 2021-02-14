namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Gives access to configuration options (typically values pulled from MSBuild). The
    /// typical implementation is <see cref="AnalyzerConfigOptionsAdapter"/>, but other implementations can be used for testing.
    /// </summary>
    public interface IConfigOptions
    {
        /// <summary>
        /// Gets a configuration value.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool TryGetValue( string name, out string? value );
    }
}