using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.LiteralExpressions.StaticallyTypedLiteralExpression
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            string staticallyTypedVariableInitializedByLiteralExpression = "literal";
            return proceed();
        }
    }
}
