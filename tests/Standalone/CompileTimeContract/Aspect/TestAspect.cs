using Contract;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Aspect
{
    public class TestAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            int? x = builder.Project.ServiceProvider.GetService<IContract>()?.Foo();

            builder.Advice.IntroduceMethod(builder.Target, nameof(Bar), args: new { i = x });
        }

        [Template]
        public static void Bar([CompileTime] int i)
        {
            Console.WriteLine($"{i}");
        }
    }
}