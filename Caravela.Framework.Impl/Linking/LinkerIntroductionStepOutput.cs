// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.CodeModel;
using Caravela.Framework.Impl.Diagnostics;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionStepOutput
    {
        public LinkerIntroductionStepOutput(
            UserDiagnosticSink diagnosticSink,
            CompilationModel finalCompilationModel,
            PartialCompilation intermediateCompilation,
            LinkerIntroductionRegistry introductionRegistry,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers)
        {
            this.DiagnosticSink = diagnosticSink;
            this.FinalCompilationModel = finalCompilationModel;
            this.IntermediateCompilation = intermediateCompilation;
            this.IntroductionRegistry = introductionRegistry;
            this.OrderedAspectLayers = orderedAspectLayers;
        }

        /// <summary>
        /// Gets the diagnostic sink.
        /// </summary>
        public UserDiagnosticSink DiagnosticSink { get; }

        /// <summary>
        /// Gets the final compilation model.
        /// </summary>
        public CompilationModel FinalCompilationModel { get; }

        /// <summary>
        /// Gets the intermediate compilation.
        /// </summary>
        public PartialCompilation IntermediateCompilation { get; }

        /// <summary>
        /// Gets the introduction registry.
        /// </summary>
        public LinkerIntroductionRegistry IntroductionRegistry { get; }

        /// <summary>
        /// Gets a list of ordered aspect layers.
        /// </summary>
        public IReadOnlyList<OrderedAspectLayer> OrderedAspectLayers { get; }
    }
}