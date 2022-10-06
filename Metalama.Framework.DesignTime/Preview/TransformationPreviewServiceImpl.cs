﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Compiler;
using Metalama.Framework.DesignTime.Contracts;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Options;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Utilities;
using Metalama.Framework.Project;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Metalama.Framework.DesignTime.Preview;

public class TransformationPreviewServiceImpl : ITransformationPreviewServiceImpl
{
    private readonly DesignTimeAspectPipelineFactory _designTimeAspectPipelineFactory;

    public TransformationPreviewServiceImpl( IServiceProvider serviceProvider )
    {
        this._designTimeAspectPipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
    }

    public async Task<PreviewTransformationResult> PreviewTransformationAsync(
        ProjectKey projectKey,
        string syntaxTreeName,
        CancellationToken cancellationToken = default )
    {
        // Get the pipeline for the compilation.
        if ( !this._designTimeAspectPipelineFactory.TryGetPipeline( projectKey, out var pipeline )
             || pipeline.LastCompilation == null )
        {
            // We cannot create the pipeline because we don't have all options.
            // If this is a problem, we will need to pass all options as AssemblyMetadataAttribute.

            return PreviewTransformationResult.Failure( "The project has not been fully loaded yet. Open a file of this project in the editor." );
        }

        // Find the syntax tree of the given name.
        var syntaxTree = pipeline.LastCompilation.SyntaxTrees.FirstOrDefault( t => t.FilePath == syntaxTreeName );

        if ( syntaxTree == null )
        {
            // This could happen during initialization if the pipeline did not receive the whole compilation yet.

            return PreviewTransformationResult.Failure( "Cannot find the syntax tree in the compilation." );
        }

        // Get a compilation _without_ generated code, and map the target symbol.
        var compilation = pipeline.LastCompilation;
        var generatedFiles = compilation.SyntaxTrees.Where( SourceGeneratorHelper.IsGeneratedFile );
        var sourceCompilation = compilation.RemoveSyntaxTrees( generatedFiles );

        var partialCompilation = PartialCompilation.CreatePartial( sourceCompilation, syntaxTree );

        // Get the pipeline configuration from the design-time pipeline.
        var getConfigurationResult = await pipeline.GetConfigurationAsync( partialCompilation, true, cancellationToken );

        if ( !getConfigurationResult.IsSuccess )
        {
            return PreviewTransformationResult.Failure(
                getConfigurationResult.Diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).Select( d => d.ToString() ).ToArray() );
        }

        var designTimeConfiguration = getConfigurationResult.Value;

        // For preview, we need to override a few options, especially to enable code formatting.
        var previewServiceProvider = designTimeConfiguration.ServiceProvider.WithService(
            new PreviewProjectOptions( designTimeConfiguration.ServiceProvider.GetRequiredService<IProjectOptions>() ) );

        var previewConfiguration = designTimeConfiguration.WithServiceProvider( previewServiceProvider );

        // Execute the compile-time pipeline with the design-time project configuration.
        var previewPipeline = new CompileTimeAspectPipeline(
            previewServiceProvider,
            false,
            this._designTimeAspectPipelineFactory.Domain,
            ExecutionScenario.Preview );

        DiagnosticBag diagnostics = new();

        var pipelineResult = await previewPipeline.ExecuteCoreAsync(
            diagnostics,
            partialCompilation,
            ImmutableArray<ManagedResource>.Empty,
            previewConfiguration,
            cancellationToken );

        if ( !pipelineResult.IsSuccess )
        {
            return PreviewTransformationResult.Failure(
                diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).Select( d => d.ToString() ).ToArray() );
        }

        var transformedSyntaxTree = pipelineResult.Value.ResultingCompilation.SyntaxTrees[syntaxTree.FilePath];
        var resultText = (await transformedSyntaxTree.GetTextAsync( cancellationToken )).ToString();

        return PreviewTransformationResult.Success( resultText );
    }
}