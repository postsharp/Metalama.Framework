using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Implicit
{
    // Tests single OverrideConstructor advice with trivial template on an implicit constructor.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template));
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
    }
}