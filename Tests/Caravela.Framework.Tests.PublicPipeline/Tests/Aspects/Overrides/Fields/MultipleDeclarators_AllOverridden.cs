using System;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.IntegrationTests.Aspects.Overrides.Fields.MultipleDeclarators_AllOverridden
{
    public class TestAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.OverrideFieldOrProperty( builder.Target.Fields.OfName( "A" ).Single(), nameof(PropertyTemplate) );
            builder.Advices.OverrideFieldOrProperty( builder.Target.Fields.OfName( "B" ).Single(), nameof(PropertyTemplate) );
            builder.Advices.OverrideFieldOrProperty( builder.Target.Fields.OfName( "C" ).Single(), nameof(PropertyTemplate) );
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