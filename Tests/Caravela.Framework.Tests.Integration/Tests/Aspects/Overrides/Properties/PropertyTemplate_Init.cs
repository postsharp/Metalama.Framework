// Ignored because not all declarations listed here are curretnly supported (the test should be be split into a few smaller ones).

using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Init
{
    // Tests single OverrideProperty aspect on init-only properties.

    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        {
            get
            {
                Console.WriteLine("This is the overridden getter.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine($"This is the overridden setter.");
                var discard = meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public int Property
        {
            init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        private int PrivateProperty
        {
            init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int ExpressionProperty
        {
            init => Console.WriteLine($"This is the original setter, setting {value}.");
        }

        [Override]
        private int PrivateExpressionProperty
        {
            init => Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }
}
