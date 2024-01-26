using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Multiple
{
    // Tests single OverrideConstructor aspect with trivial template on methods with trivial bodies.

    public class InnerOverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template));
        }

        [Template]
        public void Template()
        {
            Console.WriteLine("This is the inner override.");
            meta.Proceed();
        }
    }

    public class OuterOverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template));
        }

        [Template]
        public void Template()
        {
            Console.WriteLine("This is the outer override.");
            meta.Proceed();
        }
    }

    // <target>
    [InnerOverride]
    [OuterOverride]
    public class TargetClass
    {
        public TargetClass()
        {
            Console.WriteLine( $"This is the original constructor." );
        }
    }
}