using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Simple_Static
{
    // Tests OverrideConstructor advice with a trivial template on a static constructor.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override(builder.Target.StaticConstructor, nameof(Template));
        }

        [Template]
        public void Template()
        {
            Console.WriteLine( "This is the override." );
            meta.Proceed();
        }
    }

    // <target>
    [Override]
    public class TargetClass
    {
        static TargetClass()
        {
            Console.WriteLine("This is the original static constructor.");
        }
    }
}