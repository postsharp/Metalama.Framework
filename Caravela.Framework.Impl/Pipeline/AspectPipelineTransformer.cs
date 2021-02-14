using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Caravela.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Caravela.Framework.Impl
{
    [Transformer]
    internal sealed class AspectPipelineTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext transformerContext )
        {
            if ( CompileTimeAspectPipeline.TryExecute( new AspectPipelineContext( transformerContext ), out var compilation ) )
            {
                return compilation;
            }
            else
            {
                return transformerContext.Compilation;
            }
        }

        private class AspectPipelineContext : IAspectPipelineContext
        {
            private readonly TransformerContext _transformerContext;

            public AspectPipelineContext( TransformerContext transformerContext )
            {
                this._transformerContext = transformerContext;
                this.Options = new AnalyzerConfigOptionsAdapter( this._transformerContext.GlobalOptions );
            }

            public CSharpCompilation Compilation => (CSharpCompilation) this._transformerContext.Compilation;

            public ImmutableArray<object> Plugins => this._transformerContext.Plugins;

            public AnalyzerConfigOptions GlobalOptions => this._transformerContext.GlobalOptions;

            public IList<ResourceDescription> ManifestResources => this._transformerContext.ManifestResources;

            public IConfigOptions Options { get; }

            public CancellationToken CancellationToken => CancellationToken.None;

            public void ReportDiagnostic( Diagnostic diagnostic ) => this._transformerContext.ReportDiagnostic( diagnostic );
        }
    }
}
