// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Rpc;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.Preview;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.Preview;

public class TransformationPreviewServiceImpl : PreviewPipelineBasedService, ITransformationPreviewServiceImpl
{
    public TransformationPreviewServiceImpl( GlobalServiceProvider serviceProvider ) : base( serviceProvider ) { }

    public async Task<PreviewTransformationResult> PreviewTransformationAsync(
        ProjectKey projectKey,
        string syntaxTreeName,
        TestableCancellationToken cancellationToken = default )
    {
        var preparation = await this.PrepareExecutionAsync( projectKey, syntaxTreeName, cancellationToken );

        if ( !preparation.Success )
        {
            return PreviewTransformationResult.Failure( preparation.ErrorMessages ?? Array.Empty<string>() );
        }

        // Execute the compile-time pipeline with the design-time project configuration.
        var previewPipeline = new PreviewAspectPipeline(
            preparation.ServiceProvider!,
            ExecutionScenario.Preview,
            this.PipelineFactory.Domain );

        DiagnosticBag diagnostics = new();

        var pipelineResult = await previewPipeline.ExecutePreviewAsync(
            diagnostics,
            preparation.PartialCompilation!,
            preparation.Configuration!,
            cancellationToken );

        var errorMessages = diagnostics.Where( d => d.Severity == DiagnosticSeverity.Error ).Select( d => d.ToString() ).ToArray();

        if ( !pipelineResult.IsSuccessful )
        {
            return PreviewTransformationResult.Failure( errorMessages );
        }

        var transformedSyntaxTree = pipelineResult.Value.SyntaxTrees[preparation.SyntaxTree!.FilePath];
        var resultText = (await transformedSyntaxTree.GetTextAsync( cancellationToken )).ToString();

        return PreviewTransformationResult.Success( resultText, errorMessages );
    }

    Task<PreviewTransformationResult> ITransformationPreviewServiceImpl.PreviewTransformationAsync(
        ProjectKey projectKey,
        string syntaxTreeName,
        CancellationToken cancellationToken )
        => this.PreviewTransformationAsync( projectKey, syntaxTreeName, cancellationToken.ToTestable() );
}