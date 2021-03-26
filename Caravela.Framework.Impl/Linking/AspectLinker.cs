// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

namespace Caravela.Framework.Impl.Linking
{
    /// <summary>
    /// Transforms the initial C# compilation using all transformations and aspect ordering determined in earlier stages.
    /// </summary>
    internal partial class AspectLinker
    {

        private readonly AspectLinkerInput _input;

        public AspectLinker( AspectLinkerInput input )
        {
            this._input = input;
        }

        /// <summary>
        /// Creates a set of diagnostics and final linked compilation.
        /// </summary>
        /// <returns>Linker result.</returns>
        public AspectLinkerResult ToResult()
        {
            // First step. Adds all transformations to the compilation, resulting in intermediate compilation.
            var introductionStepOutput = LinkerIntroductionStep.Instance.Execute( this._input );

            // Second step. Count references to modified methods on semantic models of intermediate compilation.
            var analysisStepOutput = LinkerAnalysisStep.Instance.Execute(introductionStepOutput);

            // Third step. Link, inline and prune intermediate compilation. This results in the final Compilation.
            var linkingStepOutput = LinkerLinkingStep.Instance.Execute( analysisStepOutput );

            // Return the final compilation and all diagnostics from all linking steps.
            return linkingStepOutput;
        }
    }
}
