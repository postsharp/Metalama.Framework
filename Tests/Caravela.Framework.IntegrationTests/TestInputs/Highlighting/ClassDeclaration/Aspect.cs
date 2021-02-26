using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.ClassDeclaration
{
    //TODO: How to have a compile-time only declaration? See TemplateAnnotator.VisitClassDeclaration
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            return proceed();
        }
    }
}
