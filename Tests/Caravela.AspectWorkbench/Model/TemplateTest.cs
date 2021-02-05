using Caravela.TestFramework;
using Microsoft.CodeAnalysis;

namespace Caravela.AspectWorkbench.Model
{
    class TemplateTest
    {
        public SyntaxNode OriginalSyntaxRoot { get; set; }

        public TestInput Input { get; set; }

        public string ExpectedOutput { get; set; }

        // TODO: Execute and edit test methods.
        //public string TestMethod { get; set; }
    }
}
