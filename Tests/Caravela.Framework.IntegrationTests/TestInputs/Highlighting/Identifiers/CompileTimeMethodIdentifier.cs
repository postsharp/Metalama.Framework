using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Identifiers.CompileTimeMethodIdentifier
{
    class Aspect
    {
        [CompileTime]
        public void CompileTimeMethod()
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            this.CompileTimeMethod();
            return proceed();
        }
    }
}
