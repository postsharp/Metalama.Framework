// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.Diagnostics;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Pipeline
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
        /// <returns></returns>
        public abstract Task<FallibleResult<AspectPipelineResult>> ExecuteAsync(
            AspectPipelineConfiguration pipelineConfiguration,
            AspectPipelineResult input,
            IDiagnosticAdder diagnostics,
            CancellationToken cancellationToken );
    }
}