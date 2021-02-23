using Caravela.TestFramework.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.Syntax.TryCatchFinally.TryCatchFinallyCompileTime
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic Template()
        {
            int n = compileTime(1);
            try
            {
                n = 2;
            }
            catch
            {
                n = 3;
            }
            finally
            {
                n = 4;
            }

            target.Parameters[0].Value = n;
            return proceed();
        }
    }

    internal class TargetCode
    {
        private int Method(int a)
        {
            return a;
        }
    }
}