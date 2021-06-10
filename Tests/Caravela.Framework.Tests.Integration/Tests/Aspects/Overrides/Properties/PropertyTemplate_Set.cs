// Ignored because not all declarations listed here are curretnly supported (the test should be be split into a few smaller ones).

using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Set
{
    // Tests single OverrideProperty aspect on set-only properties.

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

    [TestOutput]
    internal class TargetClass
    {
        [Override]
        public int Property
        {
            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        private int PrivateProperty
        {
            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public static int Static_Property
        {
            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int ExpressionProperty
        {
            set => Console.WriteLine($"This is the original setter, setting {value}.");
        }

        [Override]
        private int PrivateExpressionProperty
        {
            set => Console.WriteLine($"This is the original setter, setting {value}.");
        }

        [Override]
        public int Static_ExpressionProperty
        {
            set => Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }
}
