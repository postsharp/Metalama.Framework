using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.Identifiers.CompileTimePropertyIdentifier
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
