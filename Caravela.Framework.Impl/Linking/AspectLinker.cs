// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

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
            // First pass. Add all transformations to the compilation, resulting in intermediate compilation.
            var introductionStep = LinkerIntroductionStep.Create( this._input );
            var introductionStepResult = introductionStep.Execute();

            // Second pass. Count references to modified methods on semantic models of intermediate compilation.
            var analysisStep = LinkerAnalysisStep.Create( introductionStepResult.IntermediateCompilation, this._input.OrderedAspectLayers, introductionStepResult.TransformationRegistry );
            var analysisStepResult = analysisStep.Execute();

            // Third pass. Link an inline intermediate compilation.
            var linkingStep = LinkerLinkingStep.Create(
                this._input.OrderedAspectLayers,
                introductionStepResult.TransformationRegistry,
                introductionStepResult.IntermediateCompilation,
                analysisStepResult.ReferenceRegistry );

            var linkingStepResult = linkingStep.Execute();

            // TODO: diagnostics.
            return new( linkingStepResult.FinalCompilation, introductionStepResult.Diagnostics.Diagnostics );
        }
    }
}
