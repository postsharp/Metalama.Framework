// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Aspects;
using Caravela.Framework.Impl.CodeModel;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// The <see cref="PipelineStage"/> that evaluates aspect sources and adds aspect instances to other steps. This step runs
    /// in a fake depth numbered -1 because it needs to run before any other step within the aspect type.
    /// </summary>
    internal class EvaluateAspectSourcesPipelineStep : PipelineStep
    {
        private readonly List<IAspectSource> _aspectSources = new();

        public EvaluateAspectSourcesPipelineStep( OrderedAspectLayer aspectLayer ) : base(
            new PipelineStepId( aspectLayer.AspectLayerId, -1 ),
            aspectLayer ) { }

        public override CompilationModel Execute(
            CompilationModel compilation,
            PipelineStepsState pipelineStepsState,
            CancellationToken cancellationToken )
        {
            var aspectInstances = this._aspectSources.SelectMany(
                    s => s.GetAspectInstances( compilation, this.AspectLayer.AspectClass, pipelineStepsState, cancellationToken ) )
                .ToList();

            pipelineStepsState.AddAspectInstances( aspectInstances );

            return compilation.WithAspectInstances( aspectInstances );
        }

        public void AddAspectSource( IAspectSource aspectSource ) => this._aspectSources.Add( aspectSource );
    }
}