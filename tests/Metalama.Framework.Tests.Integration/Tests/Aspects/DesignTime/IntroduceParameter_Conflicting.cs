#if TEST_OPTIONS
// @DesignTime
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

/*
 * Tests that when parameters appended to a constructor cause constructor to conflict with another constructor, 
 * the design-time pipeline does not generate the conflicting constructor.
 */

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_Conflicting
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach(var constructor in builder.Target.Constructors.Where(c => c.Parameters.Count == 1))
            {
                builder.Advice.IntroduceParameter(constructor, "introduced1", typeof(int), TypedConstant.Create(42));
                builder.Advice.IntroduceParameter(constructor, "introduced2", typeof(int), TypedConstant.Create("42"));
            }
        }
    }

    [Introduction]
    internal partial class TestClass
    {
        public TestClass(int param, int optional1 = 42, int optional2 = 42)
        {
        }

        public TestClass(int param)
        {
        }

        public void Foo()
        {
            _ = new TestClass(42);
            _ = new TestClass(42, 42, 42);
            _ = new TestClass(42, optional1: 42);
            _ = new TestClass(42, optional2: 42);
            _ = new TestClass(42, optional1: 42, optional2: 42);
        }
    }
}