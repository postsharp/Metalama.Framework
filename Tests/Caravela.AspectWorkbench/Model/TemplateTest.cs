using Caravela.Framework.Tests.Integration.Templating;
using Caravela.TestFramework;

namespace Caravela.AspectWorkbench.Model
{
    internal class TemplateTest
    {
        public TestInput? Input { get; set; }

        public string? ExpectedOutput { get; set; }

        public TemplatingTestRunner? TestRunner { get; set; }
    }
}
