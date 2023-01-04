﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.DesignTime.Contracts.Pipeline;
using Metalama.Framework.DesignTime.Pipeline;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Engine.Utilities.Threading;
using Microsoft.CodeAnalysis;

namespace Metalama.Framework.DesignTime.VersionNeutral;

internal sealed class TransitiveCompilationService : ITransitiveCompilationService
{
    private readonly DesignTimeAspectPipelineFactory _pipelineFactory;

    public TransitiveCompilationService( GlobalServiceProvider serviceProvider )
    {
        this._pipelineFactory = serviceProvider.GetRequiredService<DesignTimeAspectPipelineFactory>();
    }

    public async ValueTask GetTransitiveAspectManifestAsync(
        Compilation compilation,
        ITransitiveCompilationResult?[] result,
        CancellationToken cancellationToken )
    {
        // Get the pipeline.
        var pipeline = await this._pipelineFactory.GetPipelineAndWaitAsync( compilation, cancellationToken );
        
        if ( pipeline == null )
        {
            result[0] = TransitiveCompilationResult.Failed();

            return;
        }
        
        // Execute the pipeline.
        var pipelineResult = await pipeline.ExecuteAsync( compilation, cancellationToken.ToTestable() );

        if ( !pipelineResult.IsSuccessful )
        {
            result[0] = TransitiveCompilationResult.Failed();
        }
        else
        {
            result[0] = new TransitiveCompilationResult(
                true,
                pipelineResult.Value.PipelineStatus == DesignTimeAspectPipelineStatus.Paused,
                pipelineResult.Value.TransformationResult.GetSerializedTransitiveAspectManifest( pipeline.ServiceProvider ) );
        }
    }
}