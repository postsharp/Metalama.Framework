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

        public AspectLinkerResult ToResult()
        {
            // First pass. Adds all transformations to the compilation, resulting in intermediate compilation.
            var introductionStep = LinkerIntroductionStep.Create( this._input );
            var introductionStepResult = introductionStep.Execute();

            // Second pass. Count references to modified methods on semantic models of intermediate compilation.
            var analysisStep = LinkerAnalysisStep.Create( introductionStepResult.IntermediateCompilation, this._input.OrderedAspectLayers, introductionStepResult.TransformationRegistry );
            var analysisStepResult = analysisStep.Execute();

            // Third pass. Link, inline and prune intermediate compilation. This results in the final Compilation.
            var linkingStep = LinkerLinkingStep.Create(
                this._input.OrderedAspectLayers,
                introductionStepResult.TransformationRegistry,
                introductionStepResult.IntermediateCompilation,
                analysisStepResult.ReferenceRegistry );

            var linkingStepResult = linkingStep.Execute();

            // Return the final compilation and all diagnostics from all linking steps.
            return new( linkingStepResult.FinalCompilation, introductionStepResult.Diagnostics.Diagnostics );
        }
    }
}
