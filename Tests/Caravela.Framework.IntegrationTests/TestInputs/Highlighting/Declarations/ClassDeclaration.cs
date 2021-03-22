using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.Declarations.ClassDeclaration
{
    class RuntimeClass
    {
    }

    [CompileTime]
    class CompileTimeClass
    {
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            return proceed();
        }
    }
}
