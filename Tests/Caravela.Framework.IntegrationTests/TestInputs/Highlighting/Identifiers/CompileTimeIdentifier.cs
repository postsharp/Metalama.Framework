using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Identifiers.CompileTimeIdentifier
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            compileTime(0);
            return proceed();
        }
    }
}
