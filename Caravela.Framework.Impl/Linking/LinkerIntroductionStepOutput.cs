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
