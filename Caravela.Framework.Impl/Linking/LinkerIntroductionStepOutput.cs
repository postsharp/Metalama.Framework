// Copyright (c) SharpCrafters s.r.o. All rights reserved.
// This project is not open source. Please see the LICENSE.md file in the repository root for details.

using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerIntroductionStepOutput
    {
        public LinkerIntroductionStepOutput( CSharpCompilation intermediateCompilation, LinkerTransformationRegistry introductionRegistry )
        {
            this.IntermediateCompilation = intermediateCompilation;
            this.TransformationRegistry = introductionRegistry;
        }

        public CSharpCompilation IntermediateCompilation { get; }

        public LinkerTransformationRegistry TransformationRegistry { get; }
    }
}
