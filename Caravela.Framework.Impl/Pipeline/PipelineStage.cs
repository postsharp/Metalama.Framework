// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Caravela.Framework.Impl.Pipeline
{
    /// <summary>
    /// Groups a set of transformations that, within the group, do not require the Roslyn compilation
    /// to be updated.
    /// </summary>
    internal abstract class PipelineStage
    {
        /// <summary>
        /// Gets the pipeline options.
        /// </summary>
        public IAspectPipelineProperties PipelineProperties { get; }

        protected PipelineStage( IAspectPipelineProperties pipelineProperties )
        {
            this.PipelineProperties = pipelineProperties;
        }

        /// <summary>
        /// Executes the pipeline, i.e. transforms inputs into outputs.
        /// </summary>
        /// <param name="input">The inputs.</param>
        /// <param name="diagnostics"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryExecute(
            PipelineStageResult input,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? result );
    }
}