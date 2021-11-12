using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.DesignTimeFallback.ProgrammaticIntroduction
{
    public class IntroductionAspectAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.IntroduceMethod(builder.Target, nameof(Foo));
        }

        [Template]
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