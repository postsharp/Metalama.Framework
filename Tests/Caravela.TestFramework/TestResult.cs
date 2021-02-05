using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;

namespace Caravela.TestFramework
{
    public class TestResult
    {
        public List<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();

        public string TestErrorMessage { get; set; }

        public string TestException { get; set; }

        public Document TemplateDocument { get; set; }

        public SyntaxNode AnnotatedSyntaxRoot { get; set; }

        public SyntaxNode TransformedSyntaxRoot { get; set; }

        public SourceText TemplateOutputSource { get; set; }
    }
}
