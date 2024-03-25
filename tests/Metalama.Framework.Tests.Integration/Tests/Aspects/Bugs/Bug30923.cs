using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug30923
{
    public sealed class TestAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            base.BuildAspect( builder );

            foreach (var property in builder.Target.Properties.Where( p => p.Type.Is( typeof(int) ) ))
            {
                builder.Advice.Override( property, nameof(TheAnswer) );
            }
        }

        [Template]
        public int TheAnswer => 42;
    }

    // <target>
    [TestAspect]
    public readonly struct TestStruct
    {
        public TestStruct()
        {
            Test = 0;
        }

        public TestStruct( int test )
        {
            Test = test;
        }

        public int Test { get; }
    }
}