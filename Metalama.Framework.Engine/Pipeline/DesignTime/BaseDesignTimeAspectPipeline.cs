// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.CompileTime;
using Metalama.Framework.Engine.Services;
using Metalama.Framework.Services;

namespace Metalama.Framework.Engine.Pipeline.DesignTime;

public abstract class BaseDesignTimeAspectPipeline : AspectPipeline
{
    protected BaseDesignTimeAspectPipeline( ServiceProvider<IProjectService> serviceProvider, CompileTimeDomain? domain ) : base(
        serviceProvider,
        ExecutionScenario.DesignTime,
        domain ) { }

    /// <inheritdoc/>
    private protected override HighLevelPipelineStage CreateHighLevelStage(
        PipelineStageConfiguration configuration,
        CompileTimeProject compileTimeProject )
        => new DesignTimePipelineStage( compileTimeProject, configuration.AspectLayers, this.ServiceProvider );

    private protected override LowLevelPipelineStage? CreateLowLevelStage( PipelineStageConfiguration configuration ) => null;
}