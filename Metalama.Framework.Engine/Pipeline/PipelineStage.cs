// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Metalama.Framework.Impl.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Metalama.Framework.Impl.Pipeline
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
        public IServiceProvider ServiceProvider { get; }

        protected PipelineStage( IServiceProvider serviceProvider )
        {
            this.ServiceProvider = serviceProvider;
        }

        /// <summary>
        /// Executes the pipeline, i.e. transforms inputs into outputs.
        /// </summary>
        /// <param name="pipelineConfiguration"></param>
        /// <param name="input">The inputs.</param>
        /// <param name="diagnostics"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public abstract bool TryExecute(
            AspectPipelineConfiguration pipelineConfiguration,
            PipelineStageResult input,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken,
            [NotNullWhen( true )] out PipelineStageResult? result );
    }
}