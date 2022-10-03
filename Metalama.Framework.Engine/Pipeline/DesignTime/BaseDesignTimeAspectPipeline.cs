// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

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

    private protected override LowLevelPipelineStage? CreateLowLevelStage( PipelineStageConfiguration configuration ) => null;
}