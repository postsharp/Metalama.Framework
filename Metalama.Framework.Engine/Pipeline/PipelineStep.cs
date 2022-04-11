// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Engine.AspectOrdering;
using Metalama.Framework.Engine.CodeModel;
using System.Threading;

namespace Metalama.Framework.Engine.Pipeline
{
    /// <summary>
    /// A step executed by <see cref="HighLevelPipelineStage"/>.
    /// </summary>
    internal abstract class PipelineStep
    {
        public PipelineStepId Id { get; }

        public OrderedAspectLayer AspectLayer { get; }

        protected PipelineStepsState Parent { get; }

        public PipelineStep( PipelineStepsState parent, PipelineStepId id, OrderedAspectLayer aspectLayer )
        {
            this.Id = id;
            this.AspectLayer = aspectLayer;
            this.Parent = parent;
        }

        /// <summary>
        /// Executes the step.
        /// </summary>
        /// <param name="compilation"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract CompilationModel Execute(
            CompilationModel compilation,
            CancellationToken cancellationToken );
    }
}