using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Identifiers.CompileTimePropertyIdentifier
{
    class Aspect
    {
        [CompileTime]
        public int CompileTimeProperty { get; set; }


        [TestTemplate]
        dynamic Template()
        {
            this.CompileTimeProperty.ToString();
            return proceed();
        }
    }
}
