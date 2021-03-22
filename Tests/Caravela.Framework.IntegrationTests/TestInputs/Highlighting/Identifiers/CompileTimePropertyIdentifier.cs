using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Identifiers.CompileTimePropertyIdentifier
{
    [CompileTime]
    class CompileTimeClass
    {
        public int CompileTimeProperty { get; set; }
    }

    [CompileTime]

    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var compileTimeObject = new CompileTimeClass();

            compileTimeObject.CompileTimeProperty.ToString();
            return proceed();
        }
    }
}
