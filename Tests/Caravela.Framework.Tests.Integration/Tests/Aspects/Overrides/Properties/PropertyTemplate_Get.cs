using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.PropertyTemplate_Get
{
    // Tests single OverrideProperty aspect on get-only properties.

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
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public int ExpressionProperty => 42;

        [Override]
        private int PrivateExpressionProperty => 42;

        [Override]
        public static int Static_ExpressionProperty => 42;

        [Override]
        public int GetterProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
        }

        [Override]
        private int PrivateGetterProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
        }

        [Override]
        public static int Static_GetterProperty
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
        }

        [Override]
        public int GetterExpressionProperty
        {
            get => 42;
        }

        [Override]
        private int PrivateGetterExpressionProperty
        {
            get => 42;
        }

        [Override]
        public int Static_GetterExpressionProperty
        {
            get => 42;
        }
    }
}
