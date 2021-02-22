using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.DesignTime
{

    // [Generator]
    public class MySourceGenerator : ISourceGenerator
    {
        void ISourceGenerator.Execute( GeneratorExecutionContext context )
        {
            if ( Caravela.Compiler.CaravelaCompilerInfo.IsActive || context.Compilation is not CSharpCompilation )
            {
                return;
            }

            using SourceGeneratorAspectPipeline pipeline = new( new AspectPipelineContext( context ) );
            if ( pipeline.TryExecute( out var additionalSyntaxTrees ) )
            {
                foreach ( var additionalSyntaxTree in additionalSyntaxTrees )
                {
                    context.AddSource( additionalSyntaxTree.Key, additionalSyntaxTree.Value.GetText() );
                }
            }
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context )
        {
        }

        private class AspectPipelineContext : IAspectPipelineContext
        {
            private readonly GeneratorExecutionContext _generatorContext;

            public AspectPipelineContext( GeneratorExecutionContext generatorContext )
            {
                this._generatorContext = generatorContext;
                this.BuildOptions = new BuildOptions( new AnalyzerBuildOptionsSource( generatorContext.AnalyzerConfigOptions ) );
            }

            public CSharpCompilation Compilation => (CSharpCompilation) this._generatorContext.Compilation;

            public ImmutableArray<object> Plugins => ImmutableArray<object>.Empty;

            public IList<ResourceDescription> ManifestResources => ImmutableArray<ResourceDescription>.Empty;

            public IBuildOptions BuildOptions { get; }

            public CancellationToken CancellationToken => this._generatorContext.CancellationToken;

            public void ReportDiagnostic( Diagnostic diagnostic ) => this._generatorContext.ReportDiagnostic( diagnostic );

            public bool HandleExceptions => true;
        }
    }
}
