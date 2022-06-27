using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Operators.Simple
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            foreach (var o in builder.Target.Operators)
            {
                builder.Advice.Override(o, nameof(Template));
            }
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
        public static TargetClass operator +(TargetClass a, TargetClass b)
        {
            Console.WriteLine($"This is the original operator.");

            return new TargetClass();
        }

        public static explicit operator TargetClass( int x )
        {
            Console.WriteLine($"This is the original operator.");

            return new TargetClass();
        }
    }
}
