namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerAnalysisStepResult
    {
        public LinkerReferenceRegistry ReferenceRegistry { get; }

        public LinkerAnalysisStepResult( LinkerReferenceRegistry referenceRegistry )
        {
            this.ReferenceRegistry = referenceRegistry;
        }
    }
}