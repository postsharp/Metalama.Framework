using Metalama.Testing.AspectTesting;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.TestInputs.Highlighting.IfStatements.CompileTimeIfCondition
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

    [RunTimeOrCompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var runTimeObject = new RunTimeClass();
            var compileTimeObject = new CompileTimeClass();

            int compileTimeVariable = meta.CompileTime(0);

            if (compileTimeVariable == 0
                // Intentionally on three lines
                || compileTimeVariable == 1
                || compileTimeVariable == 2 )
            {
                runTimeObject.RunTimeMethod();
                compileTimeObject.CompileTimeMethod();
                meta.Target.Method.ToString();
            }
            else
            {
                runTimeObject.RunTimeMethod();
                compileTimeObject.CompileTimeMethod();
                meta.Target.Method.ToString();
            }

            return meta.Proceed();
        }
    }
}
