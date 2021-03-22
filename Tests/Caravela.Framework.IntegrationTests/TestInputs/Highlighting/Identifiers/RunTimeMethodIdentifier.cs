using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Identifiers.RunTimeMethodIdentifier
{
    class RunTimeClass
    {
        public void RunTimeMethod()
        {
        }
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var runTimeObject = new RunTimeClass();

            runTimeObject.RunTimeMethod();
            return proceed();
        }
    }
}
