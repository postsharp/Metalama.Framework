using Caravela.Framework.Aspects;

namespace Caravela.Framework.IntegrationTests.DesignTimeFallback.DeclarativeIntroduction
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
    public partial class TargetClass
    {
        public void Bar()
        {
            _ = this.Foo();
        }
    }

    // <remove>
    public partial class TargetClass
    {
        public int Foo()
        {
            return 0;
        }
    }
}