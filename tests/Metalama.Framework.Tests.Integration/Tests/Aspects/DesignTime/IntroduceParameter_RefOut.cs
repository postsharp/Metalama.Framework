#if TEST_OPTIONS
// @DesignTime
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

/*
 * Tests that when a parameter is appended to a constructor with ref/out parameters, the design-time pipeline generates
 * a constructor that uses existing constructor with correct "refness".
 */

namespace Metalama.Framework.IntegrationTests.Aspects.DesignTime.IntroduceParameter_RefOut
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var constructor in builder.Target.Constructors)
            {
                builder.Advice.IntroduceParameter( constructor, "introduced1", typeof(int), TypedConstant.Create( 42 ) );
                builder.Advice.IntroduceParameter( constructor, "introduced2", typeof(string), TypedConstant.Create( "42" ) );
            }
        }
    }

    [Introduction]
    internal partial class TestClass
    {
        public TestClass( ref int param, int optParam = 42 ) { }

        public TestClass( out string param, int optParam = 42 )
        {
            param = "42";
        }

        public void Foo()
        {
            var f = 42;
            _ = new TestClass( ref f );
            _ = new TestClass( out var g );
        }
    }
}