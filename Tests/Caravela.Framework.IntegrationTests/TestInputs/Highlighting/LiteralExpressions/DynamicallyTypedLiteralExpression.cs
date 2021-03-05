using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.LiteralExpressions.DynamicallyTypedLiteralExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            //TODO: What does it mean "unless they are converted to dynamic"?
            dynamic dynmiacallyTypedVariableInitializedByLiteralExpression = (dynamic) "literal";
            return proceed();
        }
    }
}
