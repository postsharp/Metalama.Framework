using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.InvocationExpressions.CompileTimeMethodInvocation
{
    [CompileTime]
    class CompileTimeClass
    {
        public void CompileTimeMethod()
        {
        }

        public void CompileTimeMethod(int param)
        {
        }

        public void CompileTimeMethod(int param1, int param2)
        {
        }

        public void CompileTimeMethod(int param1, int param2, int param3)
        {
        }
    }

    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var compileTimeObject = new CompileTimeClass();

            // Run-time arguments are not allowed in build-time expressions.
            int staticallyTypedCompileTimeInt = compileTime(0);
            // Dynamically-typed compile-time arguments are not allowed in build-time expressions.


            compileTimeObject.CompileTimeMethod();

            compileTimeObject.CompileTimeMethod(staticallyTypedCompileTimeInt);
            compileTimeObject.CompileTimeMethod(staticallyTypedCompileTimeInt, staticallyTypedCompileTimeInt);
            compileTimeObject.CompileTimeMethod(staticallyTypedCompileTimeInt, staticallyTypedCompileTimeInt, staticallyTypedCompileTimeInt);

            return proceed();
        }
    }
}
