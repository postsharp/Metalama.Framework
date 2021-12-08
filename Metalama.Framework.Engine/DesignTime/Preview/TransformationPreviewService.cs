// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Compiler;
using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.Impl.CodeModel;
using Metalama.Framework.Impl.DesignTime.Diff;
using Metalama.Framework.Impl.DesignTime.Pipeline;
using Metalama.Framework.Impl.Diagnostics;
using Metalama.Framework.Impl.Options;
using Metalama.Framework.Impl.Pipeline;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Impl.DesignTime.Preview
{
    public class TransformationPreviewService : ITransformationPreviewService
    {
        private readonly DesignTimeAspectPipelineFactory _designTimeAspectPipelineFactory;

        internal TransformationPreviewService( DesignTimeAspectPipelineFactory designTimeAspectPipelineFactory )
        {
            this._designTimeAspectPipelineFactory = designTimeAspectPipelineFactory;
        }

        public TransformationPreviewService() : this( DesignTimeAspectPipelineFactory.Instance ) { }

        public async Task PreviewTransformationAsync(
            Compilation compilation,
            SyntaxTree syntaxTree,
            IPreviewTransformationResult?[] result,
            CancellationToken cancellationToken )
        {
            // Get the pipeline for the compilation.
            if ( !this._designTimeAspectPipelineFactory.TryGetPipeline( compilation, out var designTimePipeline ) )
            {
                // We cannot create the pipeline because we don't have all options.
                // If this is a problem, we will need to pass all options as AssemblyMetadataAttribute.

                result[0] = PreviewTransformationResult.Failure( "The component has not been initialized yet." );

                return;
            }

            // Get a compilation _without_ generated code, and map the target symbol.
            var generatedFiles = compilation.SyntaxTrees.Where( CompilationChangeTracker.IsGeneratedFile );
            var sourceCompilation = compilation.RemoveSyntaxTrees( generatedFiles );

            var partialCompilation = PartialCompilation.CreatePartial( sourceCompilation, syntaxTree );

            DiagnosticList diagnostics = new();

            // Get the pipeline configuration from the design-time pipeline.
            if ( !designTimePipeline.TryGetConfiguration( partialCompilation, diagnostics, true, cancellationToken, out var designTimeConfiguration ) )
            {
                var error = string.Join( Environment.NewLine, diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ) );

                result[0] = PreviewTransformationResult.Failure( error );

                return;
            }

            // For preview, we need to override a few options, especially to enable code formatting.
            var previewServiceProvider = designTimeConfiguration.ServiceProvider.WithService(
                new PreviewProjectOptions( designTimeConfiguration.ServiceProvider.GetService<IProjectOptions>() ) );

            var previewConfiguration = designTimeConfiguration.WithServiceProvider( previewServiceProvider );

            // Execute the compile-time pipeline with the design-time project configuration.
            var previewPipeline = new CompileTimeAspectPipeline(
                previewServiceProvider,
                false,
                this._designTimeAspectPipelineFactory.Domain,
                ExecutionScenario.Preview );

            var pipelineResult = await previewPipeline.ExecuteCoreAsync(
                diagnostics,
                partialCompilation,
                ImmutableArray<ManagedResource>.Empty,
                previewConfiguration,
                cancellationToken );

            if ( pipelineResult == null )
            {
                var error = string.Join( Environment.NewLine, diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ) );

                result[0] = PreviewTransformationResult.Failure( error );

                return;
            }

            var transformedSyntaxTree = pipelineResult.ResultingCompilation.SyntaxTrees[syntaxTree.FilePath];

            result[0] = PreviewTransformationResult.Success( transformedSyntaxTree );
        }
    }
}