// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Engine.Utilities.Threading;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Preview;

public class PreviewPipelineBasedService
{
    private protected DesignTimeAspectPipelineFactory PipelineFactory { get; }

    public PreviewPipelineBasedService( GlobalServiceProvider serviceProvider )
    {
        this.PipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
    }

    protected async
        Task<(bool Success, string[]? ErrorMessages, SyntaxTree? SyntaxTree, ProjectServiceProvider? ServiceProvider, AspectPipelineConfiguration? Configuration
           ,
            PartialCompilation? PartialCompilation )> PrepareExecutionAsync(
            ProjectKey projectKey,
            string syntaxTreeName,
            TestableCancellationToken cancellationToken )
    {
        // Get the pipeline for the compilation.
        if ( !this.PipelineFactory.TryGetPipeline( projectKey, out var pipeline )
             || pipeline.LastCompilation == null )
        {
            // We cannot create the pipeline because we don't have all options.
            // If this is a problem, we will need to pass all options as AssemblyMetadataAttribute.

            return (false, new[] { "The project has not been fully loaded yet. Open a file of this project in the editor." }, null, null, null, null);
        }

        // Find the syntax tree of the given name.
        var syntaxTree = pipeline.LastCompilation.SyntaxTrees.FirstOrDefault( t => t.FilePath == syntaxTreeName );

        if ( syntaxTree == null )
        {
            // This could happen during initialization if the pipeline did not receive the whole compilation yet.

            return (false, new[] { "Cannot find the syntax tree in the compilation." }, null, null, null, null);
        }

        // Get a compilation _without_ generated code, and map the target symbol.
        var compilation = pipeline.LastCompilation;
        var generatedFiles = compilation.SyntaxTrees.Where( SourceGeneratorHelper.IsGeneratedFile );
        var sourceCompilation = compilation.RemoveSyntaxTrees( generatedFiles );

        var partialCompilation = PartialCompilation.CreatePartial( sourceCompilation, syntaxTree );

        // Resume all pipelines.
        await this.PipelineFactory.ResumePipelinesAsync( cancellationToken );

        // Get the pipeline configuration from the design-time pipeline.
        await pipeline.InvalidateCacheAsync( compilation, cancellationToken );
        var getConfigurationResult = await pipeline.GetConfigurationAsync( partialCompilation, true, cancellationToken );

        if ( !getConfigurationResult.IsSuccessful )
        {
            return (false, getConfigurationResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).Select( d => d.ToString() ).ToArray(), null,
                    null, null, null);
        }

        var designTimeConfiguration = getConfigurationResult.Value;

        // Get the DesignTimeProjectVersion because it implements ITransitiveProjectManifest.
        var transitiveAspectManifest = await pipeline.GetDesignTimeProjectVersionAsync( sourceCompilation, cancellationToken );

        if ( !transitiveAspectManifest.IsSuccessful )
        {
            return (false, transitiveAspectManifest.Diagnostics.Select( x => x.ToString() ).ToArray(), null, null, null, null);
        }

        // For preview, we need to override a few options, especially to enable code formatting.
        var previewServiceProvider = designTimeConfiguration.ServiceProvider
            .WithService( new PreviewProjectOptions( designTimeConfiguration.ServiceProvider.GetRequiredService<IProjectOptions>() ) )
            .WithService( transitiveAspectManifest.Value );

        var previewConfiguration = designTimeConfiguration.WithServiceProvider( previewServiceProvider );

        return (true, null, syntaxTree, previewServiceProvider, previewConfiguration, partialCompilation);
    }
}