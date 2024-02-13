#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_ExplicitParameterless
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
        public TestClass()
        {
        }

        public void Foo()
        {
            _ = new TestClass();
        }
    }
}