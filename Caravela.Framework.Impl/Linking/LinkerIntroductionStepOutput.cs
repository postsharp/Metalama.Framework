// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionStepOutput
    {
        public LinkerIntroductionStepOutput( DiagnosticList diagnostics, CSharpCompilation intermediateCompilation, LinkerTransformationRegistry introductionRegistry )
        {
            this.Diagnostics = diagnostics;
            this.IntermediateCompilation = intermediateCompilation;
            this.TransformationRegistry = introductionRegistry;
        }

        public DiagnosticList Diagnostics { get; }

        public CSharpCompilation IntermediateCompilation { get; }

        public LinkerTransformationRegistry TransformationRegistry { get; }
    }
}
