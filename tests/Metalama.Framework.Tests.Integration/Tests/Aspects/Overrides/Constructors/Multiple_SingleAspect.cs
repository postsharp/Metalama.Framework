using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Constructors.Multiple_SingleAspect
{
    // Tests that single OverrideConstructor advice that does not call meta.Proceed produces a diagnostic error.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template), args: new { order = 1 });
            builder.Advice.Override(builder.Target.Constructors.Single(), nameof(Template), args: new { order = 2 });
        }

        [Template]
        public void Template([CompileTime] int order)
        {
            Console.WriteLine( $"This is the override {order}." );

            meta.Proceed();
        }
    }

    // <target>
    [Override]
    internal class TargetClass
    {
        public TargetClass()
        {
            Console.WriteLine( $"This is the original constructor." );
        }
    }
}