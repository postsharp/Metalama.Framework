// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using System.Collections.Generic;
using Caravela.Framework.Impl.AspectOrdering;
using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionStepOutput
    {
        public LinkerIntroductionStepOutput( DiagnosticList diagnosticSink, CSharpCompilation intermediateCompilation, LinkerIntroductionRegistry introductionRegistry, IReadOnlyList<OrderedAspectLayer> orderedAspectLayers )
        {
            this.DiagnosticSink = diagnosticSink;
            this.IntermediateCompilation = intermediateCompilation;
            this.IntroductionRegistry = introductionRegistry;
            this.OrderedAspectLayers = orderedAspectLayers;
        }

        /// <summary>
        /// Gets the diagnostic sink.
        /// </summary>
        public DiagnosticList DiagnosticSink { get; }

        /// <summary>
        /// Gets the intermediate compilation.
        /// </summary>
        public CSharpCompilation IntermediateCompilation { get; }

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
