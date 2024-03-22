#if TEST_OPTIONS
// @DesignTime
#endif

using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

/*
 * Tests that when a parameter is appended to a constructor with optional parameter and a "deambiguing" constructor (a constructor with only mandatory parameters)
 * the design-time pipeline does not generate another "deambiguing" constructor.
 */

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_ExistingOptionalNonOptional
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var constructor in builder.Target.Constructors.Where(c => c.Parameters.Count == 2))
            {
                builder.Advice.IntroduceParameter(constructor, "introduced1", typeof(int), TypedConstant.Create(42));
                builder.Advice.IntroduceParameter(constructor, "introduced2", typeof(string), TypedConstant.Create("42"));
            }
        }
    }

    [Introduction]
    internal partial class TestClass
    {
        public TestClass(int param)
        {
        }

        public TestClass(int param, int optParam = 42)
        {
        }

        public void Foo()
        {
            _ = new TestClass(42);
            _ = new TestClass(param: 42);
            _ = new TestClass(42, 42);
            _ = new TestClass(42, optParam: 42);
            _ = new TestClass(optParam: 42, param: 13);
        }
    }
}