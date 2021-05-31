// Ignored because not all declarations listed here are curretnly supported (the test should be be split into a few smaller ones).

using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_GetInit
{
    // Tests single OverrideProperty aspect with trivial template on methods with trivial bodies.

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
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }

            init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int RestrictedGetProperty
        {
            private get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }

            init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int RestrictedSetProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }

            private init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int GetExpressionProperty
        {
            get => 42;

            init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int InitExpressionProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }

            init => Console.WriteLine($"This is the original setter, setting {value}.");
        }
    }
}
