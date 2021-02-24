using Caravela.Framework.Impl.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;

namespace Caravela.Framework.Impl.Linking
{
    internal class LinkerLinkingStepResult
    {
        public LinkerLinkingStepResult( CSharpCompilation finalCompilation, DiagnosticList diagnostics )
        {
            this.FinalCompilation = finalCompilation;
            this.Diagnostics = diagnostics;
        }

        public CSharpCompilation FinalCompilation { get; }
        public DiagnosticList Diagnostics { get; }
    }
}