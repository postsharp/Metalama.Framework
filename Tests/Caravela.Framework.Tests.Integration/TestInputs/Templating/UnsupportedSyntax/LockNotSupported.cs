using static Caravela.Framework.Aspects.TemplateContext;
using Caravela.Framework.Project;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.LockNotSupported
{
    [CompileTime]
    class Aspect
    {
        private static readonly object o = new object();

        [TestTemplate]
        dynamic Template()
        {
            dynamic result;
            lock (o)
            {
                result = proceed();
            }
            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}