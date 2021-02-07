using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;

namespace Caravela.TestFramework
{
    public class TestResult
    {
        public List<Diagnostic> Diagnostics { get; set; } = new List<Diagnostic>();

        public string ErrorMessage { get; set; }

        public Exception Exception { get; set; }

        public Document TemplateDocument { get; set; }

        public SyntaxNode AnnotatedTemplateSyntax { get; set; }

        public SyntaxNode TransformedTemplateSyntax { get; set; }

        public SyntaxNode TransformedTargetSyntax { get; set; }

        public SourceText TransformedTargetSource { get; set; }
    }
}
