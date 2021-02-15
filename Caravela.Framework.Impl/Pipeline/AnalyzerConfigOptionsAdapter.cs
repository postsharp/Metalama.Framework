using Microsoft.CodeAnalysis.Diagnostics;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// An implementation of <see cref="IConfigOptions"/> that reads the values from <see cref="AnalyzerConfigOptions"/>.
    /// </summary>
    internal class AnalyzerConfigOptionsAdapter : IConfigOptions
    {
        private readonly AnalyzerConfigOptions _options;

        public AnalyzerConfigOptionsAdapter( AnalyzerConfigOptionsProvider options )
        {
            this._options = options.GlobalOptions;
        }

        public AnalyzerConfigOptionsAdapter( AnalyzerConfigOptions options )
        {
            this._options = options;
        }

        public bool TryGetValue( string name, out string? value ) => this._options.TryGetValue( name, out value );
    }
}