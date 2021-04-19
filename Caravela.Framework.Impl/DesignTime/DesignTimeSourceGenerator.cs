// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Compiler;
using Caravela.Framework.Impl.Pipeline;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.DesignTime
{
    [Generator]
    public class DesignTimeSourceGenerator : ISourceGenerator
    {
        void ISourceGenerator.Execute( GeneratorExecutionContext context )
        {
            if ( CaravelaCompilerInfo.IsActive ||
                 context.Compilation is not CSharpCompilation compilation )
            {
                return;
            }

            if ( !DesignTimeAspectPipelineCache.TryGet( compilation, out var pipelineResult ) )
            {
                using DesignTimeAspectPipeline pipeline = new( new DesignTimeAspectPipelineContext(
                                                                   compilation,
                                                                   new BuildOptions( new AnalyzerBuildOptionsSource( context.AnalyzerConfigOptions ) ),
                                                                   context.ReportDiagnostic,
                                                                   context.CancellationToken ) );

                _ = pipeline.TryExecute( out pipelineResult );

                DesignTimeAspectPipelineCache.Add( compilation, pipelineResult );
            }

            foreach ( var diagnostic in pipelineResult.Diagnostics.ReportedDiagnostics )
            {
                context.ReportDiagnostic( diagnostic );
            }

            if ( pipelineResult.AdditionalSyntaxTrees != null )
            {
                foreach ( var additionalSyntaxTree in pipelineResult.AdditionalSyntaxTrees )
                {
                    context.AddSource( additionalSyntaxTree.Key, additionalSyntaxTree.Value.GetText() );
                }
            }
        }

        void ISourceGenerator.Initialize( GeneratorInitializationContext context ) { }
    }
}