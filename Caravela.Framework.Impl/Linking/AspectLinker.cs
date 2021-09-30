// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Observers;
using Caravela.Framework.Project;
using System;

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Transforms the initial C# compilation using all transformations and aspect ordering determined in earlier stages.
    /// </summary>
    internal class AspectLinker
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly AspectLinkerInput _input;

        public AspectLinker( IServiceProvider serviceProvider, AspectLinkerInput input )
        {
            this._serviceProvider = serviceProvider;
            this._input = input;
        }

        /// <summary>
        /// Creates a set of diagnostics and final linked compilation.
        /// </summary>
        /// <returns>Linker result.</returns>
        public AspectLinkerResult ToResult()
        {
            // First step. Adds all transformations to the compilation, resulting in intermediate compilation.
            var introductionStepOutput = new LinkerIntroductionStep( this._serviceProvider ).Execute( this._input );
            this._serviceProvider.GetOptionalService<ILinkerObserver>()?.OnIntermediateCompilationCreated( introductionStepOutput.IntermediateCompilation );

            // Second step. Count references to modified methods on semantic models of intermediate compilation and analyze method bodies.
            var analysisStepOutput = LinkerAnalysisStep.Instance.Execute( introductionStepOutput );

            // Third step. Link, inline and prune intermediate compilation. This results in the final compilation.
            var linkingStepOutput = LinkerLinkingStep.Instance.Execute( analysisStepOutput );

            // Return the final compilation and all diagnostics from all linking steps.
            return linkingStepOutput;
        }
    }
}