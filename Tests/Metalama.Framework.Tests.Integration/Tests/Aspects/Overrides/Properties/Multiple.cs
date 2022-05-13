using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple;

[assembly: AspectOrder( typeof(FirstOverrideAttribute), typeof(SecondOverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple
{
    /*
     * Tests that multiple aspects overriding the same property produce correct code.
     */

    // TODO: Also add introduced properties.
    // TODO: multiple aspects on get-only auto properties.

    public class FirstOverrideAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
        {
            builder.Advice.Override(builder.Target, nameof(Template));
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                Console.WriteLine("First override.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("First override.");
                meta.Proceed();
            }
        }
    }

    public class SecondOverrideAttribute : FieldOrPropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
        {
            builder.Advice.Override(builder.Target, nameof(Template));
        }

        [Template]
        public dynamic? Template
        {
            get
            {
                Console.WriteLine("Second override.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("Second override.");
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        private int _field;

        [FirstOverride]
        [SecondOverride]
        public int Property
        {
            get
            {
                return _field;
            }

            set
            {
                _field = value;
            }
        }

        private static int _staticField;

        [FirstOverride]
        [SecondOverride]
        public static int StaticProperty
        {
            get
            {
                return _staticField;
            }

            set
            {
                _staticField = value;
            }
        }

        [FirstOverride]
        [SecondOverride]
        public int ExpressionBodiedProperty => 42;


        [FirstOverride]
        [SecondOverride]
        public int AutoProperty { get; set; }

        //[FirstOverride]
        //[SecondOverride]
        //public int GetOnlyAutoProperty { get; }

        [FirstOverride]
        [SecondOverride]
        public int InitializerAutoProperty { get; set; } = 42;

        public TargetClass()
        {
            //this.GetOnlyAutoProperty = 42;
        }
    }
}