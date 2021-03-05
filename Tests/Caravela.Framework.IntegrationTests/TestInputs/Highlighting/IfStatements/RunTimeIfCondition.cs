using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.IfStatements.RunTimeIfCondition
{
    class Aspect
    {
        private int runTimeField = 1;

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
            if (this.runTimeField == 1)
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
