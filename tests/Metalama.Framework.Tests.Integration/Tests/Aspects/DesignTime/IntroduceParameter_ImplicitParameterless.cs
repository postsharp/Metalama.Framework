#if TEST_OPTIONS
// @DesignTime
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

/*
 * Tests that when a parameter is appended to a implicit parameterless constructor, the design-time pipeline generates a correct constructor with optional parameters.
 */

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_ImplicitParameterless
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var constructor in builder.Target.Constructors)
            {
                builder.Advice.IntroduceParameter(constructor, "introduced1", typeof(int), TypedConstant.Create(42));
                builder.Advice.IntroduceParameter(constructor, "introduced2", typeof(string), TypedConstant.Create("42"));
            }
        }
    }

    [Introduction]
    internal partial class TestClass
    {
        public void Foo()
        {
            _ = new TestClass();
        }
    }
}