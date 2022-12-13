// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.Aspects;
using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline;

/// <summary>
/// The <see cref="PipelineStage"/> that evaluates aspect sources and adds aspect instances to other steps. This step runs
/// in a fake depth numbered -1 because it needs to run before any other step within the aspect type.
/// </summary>
internal sealed class EvaluateAspectSourcesPipelineStep : PipelineStep
{
    private readonly ConcurrentLinkedList<IAspectSource> _aspectSources = new();

    public EvaluateAspectSourcesPipelineStep( PipelineStepsState parent, OrderedAspectLayer aspectLayer ) : base(
        parent,
        new PipelineStepId( aspectLayer.AspectLayerId, -1, -1, PipelineStepPhase.Initialize, -1 ),
        aspectLayer ) { }

    public override Task<CompilationModel> ExecuteAsync(
        CompilationModel compilation,
        int stepIndex,
        CancellationToken cancellationToken )
    {
        var aspectClass = this.AspectLayer.AspectClass;

        var concreteAspectInstances = this.Parent.ExecuteAspectSource( compilation, aspectClass, this._aspectSources, cancellationToken );

        if ( concreteAspectInstances.IsDefaultOrEmpty )
        {
            return Task.FromResult( compilation );
        }
        else
        {
            return Task.FromResult( compilation.WithTransformationsAndAspectInstances( null, concreteAspectInstances ) );
        }
    }

    public void AddAspectSource( IAspectSource aspectSource ) => this._aspectSources.Add( aspectSource );
}