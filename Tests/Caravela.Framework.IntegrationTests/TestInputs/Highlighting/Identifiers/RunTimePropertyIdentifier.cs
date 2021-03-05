using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Identifiers.RunTimePropertyIdentifier
{
    class Aspect
    {
        //TODO: Why is this highlighted?
        public int RunTimeProperty { get; set; }


        [TestTemplate]
        dynamic Template()
        {
            this.RunTimeProperty.ToString();
            return proceed();
        }
    }
}
