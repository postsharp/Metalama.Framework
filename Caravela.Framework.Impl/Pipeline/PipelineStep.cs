// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// A step executed by <see cref="HighLevelPipelineStage"/>.
    /// </summary>
    internal abstract class PipelineStep
    {
        public PipelineStepId Id { get; }

        public OrderedAspectLayer AspectLayer { get; }

        public PipelineStep( PipelineStepId id, OrderedAspectLayer aspectLayer )
        {
            this.Id = id;
            this.AspectLayer = aspectLayer;
        }

        /// <summary>
        /// Executes the step.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="pipelineStepsState"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract CompilationModel Execute( CompilationModel compilation, PipelineStepsState pipelineStepsState, CancellationToken cancellationToken );
    }
}