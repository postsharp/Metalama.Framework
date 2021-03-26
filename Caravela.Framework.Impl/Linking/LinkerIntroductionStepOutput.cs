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

        public DiagnosticList DiagnosticSink { get; }

        public CSharpCompilation IntermediateCompilation { get; }

        public LinkerIntroductionRegistry IntroductionRegistry { get; }

        public IReadOnlyList<OrderedAspectLayer> OrderedAspectLayers { get; }
    }
}
