using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.MethodSetTemplate
{
    // Tests get-only property template.

    [AttributeUsage(AttributeTargets.Property)]
    public class OverrideAttribute : Attribute, IAspect<IFieldOrProperty>
    {
        public void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
        {
            builder.AdviceFactory.OverrideFieldOrPropertyAccessors(builder.TargetDeclaration, null, nameof(SetProperty));
        }

        [Template]
        public dynamic? SetProperty()
        {
            Console.WriteLine($"This is the overridden setter.");
            return meta.Proceed();
        }
    }

    [TestOutput]
    internal class TargetClass
    {
        [Override]
        public int AutoProperty { get; set; }

        [Override]
        public static int Static_AutoProperty { get; set; }

        [Override]
        public int AutoProperty_Init { get; init; }

        [Override]
        public int AutoProperty_GetOnly { get; }

        [Override]
        public int Property
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }

            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public static int Static_Property
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }

            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int InitProperty
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
        public int Property_GetOnly
        {
            get
            {
                Console.WriteLine("This is the original getter.");
                return 42;
            }
        }

        [Override]
        public int Property_SetOnly
        {
            set
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }

        [Override]
        public int Property_InitOnly
        {
            init
            {
                Console.WriteLine($"This is the original setter, setting {value}.");
            }
        }
    }
}
