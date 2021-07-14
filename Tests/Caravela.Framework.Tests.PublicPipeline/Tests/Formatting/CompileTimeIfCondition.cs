using Caravela.TestFramework;
using Caravela.Framework.Aspects;

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
        dynamic? Template()
        {
            var runTimeObject = new RunTimeClass();
            var compileTimeObject = new CompileTimeClass();

            int compileTimeVariable = meta.CompileTime(0);

            if (compileTimeVariable == 0)
            {
                runTimeObject.RunTimeMethod();
                compileTimeObject.CompileTimeMethod();
                meta.Method.ToString();
            }
            else
            {
                runTimeObject.RunTimeMethod();
                compileTimeObject.CompileTimeMethod();
                meta.Method.ToString();
            }

            return meta.Proceed();
        }
    }
}
