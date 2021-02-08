using System.Collections.Generic;
using System.Collections.Immutable;
using Caravela.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Caravela.Framework.Impl
{
    [Transformer]
    internal sealed class AspectPipelineTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext transformerContext )
        {
            return new AspectPipeline().Execute( new AspectPipelineContext( transformerContext ) );
        }

        private class AspectPipelineContext : IAspectPipelineContext
        {
            private readonly TransformerContext _transformerContext;

            public AspectPipelineContext( TransformerContext transformerContext )
            {
                this._transformerContext = transformerContext;
            }

            public Compilation Compilation => this._transformerContext.Compilation;
            public ImmutableArray<object> Plugins => this._transformerContext.Plugins;
            public AnalyzerConfigOptions GlobalOptions => this._transformerContext.GlobalOptions;
            public IList<ResourceDescription> ManifestResources => this._transformerContext.ManifestResources;

            public bool GetOptionsFlag( string flagName ) =>
                this._transformerContext.GlobalOptions.TryGetValue( $"build_property.{flagName}", out var flagString ) &&
                bool.TryParse( flagString, out bool flagValue ) &&
                flagValue;

            public void ReportDiagnostic( Diagnostic diagnostic )
            {
                this._transformerContext.ReportDiagnostic( diagnostic );
            }
        }
    }
}
