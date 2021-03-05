using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Identifiers.TargetIdentifier
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            target.ToString();
            return proceed();
        }
    }
}
