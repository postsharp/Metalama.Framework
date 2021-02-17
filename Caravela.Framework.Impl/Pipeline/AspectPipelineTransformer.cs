using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Caravela.Compiler;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The main compile-time entry point of Caravela. An implementation of Caravela.Compiler's <see cref="ISourceTransformer"/>.
    /// </summary>
    [Transformer]
    internal sealed class AspectPipelineTransformer : ISourceTransformer
    {
        public Compilation Execute( TransformerContext transformerContext )
        {
            using CompileTimeAspectPipeline pipeline = new( new AspectPipelineContext( transformerContext ));
            if ( pipeline.TryExecute( out var compilation ) )
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
                this.BuildOptions = new BuildOptions( new AnalyzerBuildOptionsSource( this._transformerContext.GlobalOptions ) );
            }

            public CSharpCompilation Compilation => (CSharpCompilation) this._transformerContext.Compilation;

            public ImmutableArray<object> Plugins => this._transformerContext.Plugins;

            public IList<ResourceDescription> ManifestResources => this._transformerContext.ManifestResources;

            public IBuildOptions BuildOptions { get; }

            public CancellationToken CancellationToken => CancellationToken.None;

            public void ReportDiagnostic( Diagnostic diagnostic ) => this._transformerContext.ReportDiagnostic( diagnostic );

            public bool HandleExceptions => true;
        }
    }
}
