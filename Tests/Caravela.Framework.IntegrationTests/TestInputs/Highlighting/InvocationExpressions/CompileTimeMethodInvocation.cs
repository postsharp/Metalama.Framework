using Caravela.Framework.Project;
using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.InvocationExpressions.CompileTimeMethodInvocation
{
    class Aspect
    {
        [CompileTime]
        public void CompileTimeMethod()
        {
        }

        [CompileTime]
        public void CompileTimeMethod(int param)
        {
        }

        [CompileTime]
        public void CompileTimeMethod(int param1, int param2)
        {
        }

        [CompileTime]
        public void CompileTimeMethod(int param1, int param2, int param3)
        {
        }

        [TestTemplate]
        dynamic Template()
        {
            //TODO: How to test this?
            int staticInt = 0;
            dynamic dynamicInt = 1;


            this.CompileTimeMethod();

            this.CompileTimeMethod(staticInt);
            this.CompileTimeMethod(staticInt, staticInt);
            this.CompileTimeMethod(staticInt, staticInt, staticInt);

            this.CompileTimeMethod(dynamicInt);
            this.CompileTimeMethod(dynamicInt, dynamicInt);
            this.CompileTimeMethod(dynamicInt, dynamicInt, dynamicInt);

            return proceed();
        }
    }
}
