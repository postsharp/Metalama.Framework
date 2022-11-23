// Copyright (c) SharpCrafters s.r.o. See the LICENSE.md file in the root directory of this repository root for details.

using Metalama.Framework.Engine.CodeModel;
using Metalama.Framework.Engine.Observers;
using Metalama.Framework.Project;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Metalama.Framework.Engine.Linking
{
    /// <summary>
    /// Transforms the initial C# compilation using all transformations and aspect ordering determined in earlier stages.
    /// </summary>
    internal class AspectLinker
    {
        private readonly CompilationServices _compilationServices;
        private readonly AspectLinkerInput _input;

        public AspectLinker( AspectLinkerInput input )
        {
            this._compilationServices = input.CompilationModel.CompilationServices;
            this._input = input;
        }

        /// <summary>
        /// Creates a set of diagnostics and final linked compilation.
        /// </summary>
        /// <returns>Linker result.</returns>
        public async Task<AspectLinkerResult> ExecuteAsync( CancellationToken cancellationToken )
        {
            // First step. Adds all transformations to the compilation, resulting in intermediate compilation.
            var injectionStepOutput = await new LinkerInjectionStep( this._compilationServices ).ExecuteAsync( this._input, cancellationToken );

            this._compilationServices.ServiceProvider.GetService<ILinkerObserver>()
                ?.OnIntermediateCompilationCreated( injectionStepOutput.IntermediateCompilation );

            // Second step. Count references to modified methods on semantic models of intermediate compilation and analyze method bodies.
            var analysisStepOutput =
                await new LinkerAnalysisStep( this._compilationServices.ServiceProvider ).ExecuteAsync( injectionStepOutput, cancellationToken );

            // Third step. Link, inline and prune intermediate compilation. This results in the final compilation.
            var linkingStepOutput = await new LinkerLinkingStep( this._compilationServices ).ExecuteAsync( analysisStepOutput, cancellationToken );

            // Return the final compilation and all diagnostics from all linking steps.
            return linkingStepOutput;
        }
    }
}