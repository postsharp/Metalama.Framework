using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Caravela.Framework.Impl
{
    

    public interface IAspectPipelineContext
    {
        CancellationToken  CancellationToken { get; }

        CSharpCompilation Compilation { get; }

        ImmutableArray<object> Plugins { get; }

        IList<ResourceDescription> ManifestResources { get; }

        IConfigOptions Options { get; }

        void ReportDiagnostic( Diagnostic diagnostic );
    }

    public interface IConfigOptions
    {
        bool TryGetValue( string name, out string? value);
    }

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

    public static class OptionsExtensions
    {
        public static bool GetBooleanOption( this IConfigOptions options, string name, bool defaultValue = false )
        {
            if ( options.TryGetValue( $"build_property.{name}", out var flagString ) && bool.TryParse( flagString, out var flagValue ) )
            {
                return flagValue;
            }
            else
            {
                return defaultValue;
            }

        }
    }
}
