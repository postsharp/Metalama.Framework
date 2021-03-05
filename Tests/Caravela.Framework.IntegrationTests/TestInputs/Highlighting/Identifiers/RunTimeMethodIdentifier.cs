using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Identifiers.RunTimeMethodIdentifier
{
    class Aspect
    {
        //TODO: Why is this highlighted?
        public void RunTimeMethod()
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            this.RunTimeMethod();
            return proceed();
        }
    }
}
