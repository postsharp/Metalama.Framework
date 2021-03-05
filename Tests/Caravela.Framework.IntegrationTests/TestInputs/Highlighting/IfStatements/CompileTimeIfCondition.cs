using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.IfStatements.CompileTimeIfCondition
{
    class Aspect
    {
        void RuntimeMethod()
        {
        }

        [CompileTime]
        void CompileTimeMethod()
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            int compileTimeVariable = 0;

            //TODO: Should this be marked as compile-time?
            if (compileTimeVariable == 0)
            {
                this.RuntimeMethod();
                this.CompileTimeMethod();
                target.Method.ToString();
            }
            else
            {
                this.RuntimeMethod();
                this.CompileTimeMethod();
                target.Method.ToString();
            }

            return proceed();
        }
    }
}
