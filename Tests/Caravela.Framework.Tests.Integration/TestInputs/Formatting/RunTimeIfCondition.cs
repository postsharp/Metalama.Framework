using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.RunTimeIfCondition
{
    class RunTimeClass
    {
        public void RunTimeMethod()
        {
        }
    }

    [CompileTime]
    class CompileTimeClass
    {
        public void CompileTimeMethod()
        {
        }
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var runTimeObject = new RunTimeClass();
            var compileTimeObject = new CompileTimeClass();

            int runTimeVariable = 1;

            if (runTimeVariable == 1)
            {
                runTimeObject.RunTimeMethod();
                compileTimeObject.CompileTimeMethod();
                target.Method.ToString();
            }
            else
            {
                runTimeObject.RunTimeMethod();
                compileTimeObject.CompileTimeMethod();
                target.Method.ToString();
            }

            return proceed();
        }
    }
}
