// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Diagnostics;
using Caravela.Framework.Sdk;
using System.Collections.Generic;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionStepOutput
    {
        public LinkerIntroductionStepOutput(
            ImmutableDiagnosticList diagnostics,
            PartialCompilation intermediateCompilation,
            LinkerIntroductionRegistry introductionRegistry,
            IReadOnlyList<OrderedAspectLayer> orderedAspectLayers )
        {
            this.Diagnostics = diagnostics;
            this.IntermediateCompilation = intermediateCompilation;
            this.IntroductionRegistry = introductionRegistry;
            this.OrderedAspectLayers = orderedAspectLayers;
        }

        /// <summary>
        /// Gets the diagnostic sink.
        /// </summary>
        public ImmutableDiagnosticList Diagnostics { get; }

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