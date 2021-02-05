using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Caravela.TestFramework.Templating
{
    public class TestResult
    {
        public List<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();

        public string? TestErrorMessage { get; set; }

        public string? TestException { get; set; }

        public Document TemplateDocument { get; }

        public TestResult( Document templateDocument )
        {
            this.TemplateDocument = templateDocument;
        }

        public SyntaxNode? AnnotatedSyntaxRoot { get; set; }

        public SyntaxNode? TransformedSyntaxRoot { get; set; }

        public SourceText? TemplateOutputSource { get; set; }
    }
}
