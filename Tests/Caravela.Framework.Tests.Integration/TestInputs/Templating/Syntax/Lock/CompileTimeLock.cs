using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lock.CompileTimeLock
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            lock ( target.Compilation )
            {
                return proceed();
            }
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}