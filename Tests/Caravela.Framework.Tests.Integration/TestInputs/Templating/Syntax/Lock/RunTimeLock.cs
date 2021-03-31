using static Caravela.Framework.Aspects.TemplateContext;

// TODO: Change the namespace
namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Lock.RunTimeLock
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            lock ( target.This )
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
