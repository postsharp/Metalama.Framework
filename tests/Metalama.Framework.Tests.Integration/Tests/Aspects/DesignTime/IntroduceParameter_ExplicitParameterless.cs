#if TEST_OPTIONS
// @DesignTime
#endif

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

/*
 * Tests that when a parameter is appended to a explicit parameterless constructor, the design-time pipeline generates a correct constructor with optional parameters.
 */

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_ExplicitParameterless
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var constructor in builder.Target.Constructors)
            {
                builder.With( constructor ).IntroduceParameter( "introduced1", typeof(int), TypedConstant.Create( 42 ) );
                builder.With( constructor ).IntroduceParameter( "introduced2", typeof(string), TypedConstant.Create( "42" ) );
            }
        }
    }

    [Introduction]
    internal partial class TestClass
    {
        public TestClass() { }

        public void Foo()
        {
            _ = new TestClass();
        }
    }
}