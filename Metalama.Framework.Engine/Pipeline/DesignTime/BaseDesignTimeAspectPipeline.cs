// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

public abstract class BaseDesignTimeAspectPipeline : AspectPipeline
{
    public BaseDesignTimeAspectPipeline( ServiceProvider serviceProvider, bool isTest, CompileTimeDomain? domain ) : base(
        serviceProvider,
        ExecutionScenario.DesignTime,
        isTest,
        domain ) { }

    /// <inheritdoc/>
    private protected override HighLevelPipelineStage CreateHighLevelStage(
        PipelineStageConfiguration configuration,
        CompileTimeProject compileTimeProject )
        => new DesignTimePipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );

    private protected override LowLevelPipelineStage? CreateLowLevelStage( PipelineStageConfiguration configuration, CompileTimeProject compileTimeProject )
        => null;
}