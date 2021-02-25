using System;
using Microsoft.CodeAnalysis;

namespace Caravela.Framework.Impl.Linking
{
    internal partial class AspectLinker
    {

        private readonly AspectLinkerInput _input;

        public AspectLinker( AspectLinkerInput input )
        {
            this._input = input;
        }

        public AspectLinkerResult ToResult()
        {
            // First pass. Add all transformations to the compilation, but we don't link them yet.
            var introductionStep = LinkerIntroductionStep.Create( this._input );
            var introductionStepResult = introductionStep.Execute();

            // Second pass. Count references to modified methods.
            var analysisStep = LinkerAnalysisStep.Create( this._input.Compilation, this._input.OrderedAspectParts );
            var analysisStepResult = analysisStep.Execute();

            // Third pass. Linking.
            var linkingStep = LinkerLinkingStep.Create( this._input.OrderedAspectParts, introductionStepResult.TransformationRegistry, introductionStepResult.IntermediateCompilation, analysisStepResult.ReferenceRegistry );
            var linkingStepResult = linkingStep.Execute();

            // TODO: diagnostics.
            return new( linkingStepResult.FinalCompilation, Array.Empty<Diagnostic>() );
        }
    }
}
