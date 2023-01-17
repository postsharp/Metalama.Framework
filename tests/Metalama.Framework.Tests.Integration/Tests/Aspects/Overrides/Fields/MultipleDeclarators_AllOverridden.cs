using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.MultipleDeclarators_AllOverridden
{
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advise.Override( builder.Target.Fields.OfName( "A" ).Single(), nameof(PropertyTemplate) );
            builder.Advise.Override( builder.Target.Fields.OfName( "B" ).Single(), nameof(PropertyTemplate) );
            builder.Advise.Override( builder.Target.Fields.OfName( "C" ).Single(), nameof(PropertyTemplate) );
        }

        [Template]
        public dynamic? PropertyTemplate
        {
            get
            {
                Console.WriteLine( "This is aspect code." );

                return meta.Proceed();
            }
            set
            {
                Console.WriteLine( "This is aspect code." );
                meta.Proceed();
            }
        }
    }

    // <target>
    [Test]
    internal class TargetClass
    {
        // Comment before.
        public int A, B, C;

        // Comment after.
    }
}