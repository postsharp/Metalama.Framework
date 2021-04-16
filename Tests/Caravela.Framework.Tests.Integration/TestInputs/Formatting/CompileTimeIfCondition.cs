using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.CompileTimeIfCondition
{
    class RunTimeClass
    {
        public void RunTimeMethod()
        {
        }
    }

    [CompileTimeOnly]
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

            int compileTimeVariable = compileTime(0);

            if (compileTimeVariable == 0)
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
