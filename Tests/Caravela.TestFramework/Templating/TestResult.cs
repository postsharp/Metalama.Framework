using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Caravela.TestFramework.Templating
{
    public class TestResult
    {
        public List<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();

        public string TestErrorMessage { get; set; }

        public string TestException { get; set; }

        public Document InputDocument { get; set; }
        
        public SyntaxNode AnnotatedSyntaxRoot { get; set; }

        public SyntaxNode TransformedSyntaxRoot { get; set; }
        
        public SourceText TemplateOutputSource { get; set; }
    }
}
