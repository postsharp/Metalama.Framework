using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.NonInlineable_Static
{
    // Tests that single static OverrideConstructor aspect with non-inlineable template produces a diagnostic error.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override( builder.Target.StaticConstructor, nameof(Template) );
        }

        [Template]
        public void Template()
        {
            Console.WriteLine( "This is the override." );

            meta.Proceed();
            meta.Proceed();
        }
    }

    // <target>
    [Override]
    internal class TargetClass
    {
        static TargetClass()
        {
            Console.WriteLine( $"This is the original constructor." );
        }
    }
}