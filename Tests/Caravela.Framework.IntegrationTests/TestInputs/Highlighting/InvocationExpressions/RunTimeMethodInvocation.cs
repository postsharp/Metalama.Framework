using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.TestInputs.Highlighting.InvocationExpressions.RunTimeMethodInvocation
{
    class Aspect
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

        [TestTemplate]
        dynamic Template()
        {
            //TODO: highlighted as compile time variables here, but not when used as arguments.
            int staticInt = 0;
            dynamic dynamicInt = 1;


            this.RunTimeMethod();

            this.RunTimeMethod(staticInt);
            this.RunTimeMethod(staticInt, staticInt);
            this.RunTimeMethod(staticInt, staticInt, staticInt);

            this.RunTimeMethod(dynamicInt);
            this.RunTimeMethod(dynamicInt, dynamicInt);
            this.RunTimeMethod(dynamicInt, dynamicInt, dynamicInt);

            return proceed();
        }
    }
}
