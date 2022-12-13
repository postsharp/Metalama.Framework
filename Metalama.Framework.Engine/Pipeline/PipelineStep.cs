// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// A step executed by <see cref="HighLevelPipelineStage"/>.
/// </summary>
internal abstract class PipelineStep
{
    public PipelineStepId Id { get; }

    public OrderedAspectLayer AspectLayer { get; }

    protected PipelineStepsState Parent { get; }

    protected PipelineStep( PipelineStepsState parent, PipelineStepId id, OrderedAspectLayer aspectLayer )
    {
        this.Id = id;
        this.AspectLayer = aspectLayer;
        this.Parent = parent;
    }

    /// <summary>
    /// Executes the step.
    /// </summary>
    /// <param name="compilation"></param>
    /// <param name="stepIndex"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public abstract Task<CompilationModel> ExecuteAsync(
        CompilationModel compilation,
        int stepIndex,
        CancellationToken cancellationToken );

    public override string ToString() => this.Id.ToString();
}