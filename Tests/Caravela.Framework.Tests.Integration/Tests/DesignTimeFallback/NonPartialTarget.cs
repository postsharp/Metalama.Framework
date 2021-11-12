using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.DesignTimeFallback.NonPartialTarget
{
    public class IntroductionAspectAttribute : TypeAspect
    {
        [Introduce]
        public int Foo()
        {
            return 42;
        }
    }
    
    [IntroductionAspect]
    public class TargetClass
    {
        public void Bar()
        {
        }
    }
}