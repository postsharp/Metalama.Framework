using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Finalizers.Simple_Introduced
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.Override(builder.Target.Finalizer!, nameof(Template));
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine("This is the override.");
            return meta.Proceed();
        }
    }

    // <target>
    [Override]
    internal class TargetClass
    {
    }
}
