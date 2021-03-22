using Caravela.Framework.Project;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.TestInputs.Highlighting.InvocationExpressions.RunTimeMethodInvocation
{
    class RunTimeClass
    {
        public void RunTimeMethod()
        {
        }

        public void RunTimeMethod(int param)
        {
        }

        public void RunTimeMethod(int param1, int param2)
        {
        }

        public void RunTimeMethod(int param1, int param2, int param3)
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

            int staticallyTypedRunTimeInt = 0;
            dynamic dynamicallyTypedRunTimeInt = 1;
            int staticallyTypedCompileTimeInt = compileTime(0);
            dynamic dynamicallyTypedCompileTimeInt = compileTime(1);


            runTimeObject.RunTimeMethod();

            runTimeObject.RunTimeMethod(staticallyTypedRunTimeInt);
            runTimeObject.RunTimeMethod(staticallyTypedRunTimeInt, staticallyTypedRunTimeInt);
            runTimeObject.RunTimeMethod(staticallyTypedRunTimeInt, staticallyTypedRunTimeInt, staticallyTypedRunTimeInt);

            runTimeObject.RunTimeMethod(dynamicallyTypedRunTimeInt);
            runTimeObject.RunTimeMethod(dynamicallyTypedRunTimeInt, dynamicallyTypedRunTimeInt);
            runTimeObject.RunTimeMethod(dynamicallyTypedRunTimeInt, dynamicallyTypedRunTimeInt, dynamicallyTypedRunTimeInt);

            runTimeObject.RunTimeMethod(staticallyTypedCompileTimeInt);
            runTimeObject.RunTimeMethod(staticallyTypedCompileTimeInt, staticallyTypedCompileTimeInt);
            runTimeObject.RunTimeMethod(staticallyTypedCompileTimeInt, staticallyTypedCompileTimeInt, staticallyTypedCompileTimeInt);

            runTimeObject.RunTimeMethod(dynamicallyTypedCompileTimeInt);
            runTimeObject.RunTimeMethod(dynamicallyTypedCompileTimeInt, dynamicallyTypedCompileTimeInt);
            runTimeObject.RunTimeMethod(dynamicallyTypedCompileTimeInt, dynamicallyTypedCompileTimeInt, dynamicallyTypedCompileTimeInt);

            return proceed();
        }
    }
}
