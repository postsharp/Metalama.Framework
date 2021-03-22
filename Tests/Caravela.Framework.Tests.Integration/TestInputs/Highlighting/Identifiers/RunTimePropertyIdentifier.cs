using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.Identifiers.RunTimePropertyIdentifier
{
    class RunTimeClass
    {
        public int RunTimeProperty { get; set; }
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var runTimeObject = new RunTimeClass();

            runTimeObject.RunTimeProperty.ToString();
            return proceed();
        }
    }
}
