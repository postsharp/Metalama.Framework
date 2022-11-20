﻿// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Diagnostics;
using Metalama.Framework.Engine.Pipeline.CompileTime;
using Metalama.Framework.Engine.Utilities.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline.Preview;

public class PreviewAspectPipeline : AspectPipeline
{
    public PreviewAspectPipeline( ServiceProvider serviceProvider, ExecutionScenario executionScenario, bool isTest, CompileTimeDomain? domain ) : base(
        serviceProvider,
        executionScenario,
        isTest,
        domain ) { }

    private protected override HighLevelPipelineStage CreateHighLevelStage(
        PipelineStageConfiguration configuration,
        CompileTimeProject compileTimeProject )
        => new LinkerPipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );

    public async Task<FallibleResult<PartialCompilation>> ExecutePreviewAsync(
        DiagnosticBag diagnostics,
        PartialCompilation compilation,
        AspectPipelineConfiguration configuration,
        TestableCancellationToken cancellationToken )
    {
        var result = await this.ExecuteAsync( compilation, diagnostics, configuration, cancellationToken );

        if ( result.IsSuccessful )
        {
            return result.Value.Compilation;
        }
        else
        {
            return FallibleResult<PartialCompilation>.Failed;
        }
    }
}