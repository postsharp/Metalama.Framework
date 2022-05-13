using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple_MethodTemplates;

[assembly: AspectOrder( typeof(FirstOverrideAttribute), typeof(SecondOverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple_MethodTemplates
{
    /*
     * Tests that multiple aspects overriding the same property using method templates produce correct code.
     */

    // TODO: multiple aspects on get-only auto properties.

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class FirstOverrideAttribute : PropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advice.OverrideAccessors(builder.Target, nameof(GetTemplate), nameof(SetTemplate));
        }

        [Template]
        public dynamic? GetTemplate()
        {
            Console.WriteLine("This is the overridden getter.");
            return meta.Proceed();
        }

        [Template]
        public void SetTemplate()
        {
            Console.WriteLine("This is the overridden setter.");
            meta.Proceed();
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class SecondOverrideAttribute : PropertyAspect
    {
        public override void BuildAspect(IAspectBuilder<IProperty> builder)
        {
            builder.Advice.OverrideAccessors(builder.Target, nameof(GetTemplate), nameof(SetTemplate));
        }

        [Template]
        public dynamic? GetTemplate()
        {
            Console.WriteLine("This is the overridden getter.");
            _ = meta.Proceed();

            return meta.Proceed();
        }

        [Template]
        public void SetTemplate()
        {
            Console.WriteLine("This is the overridden setter.");
            meta.Proceed();
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
            // this.GetOnlyAutoProperty = 42;
        }
    }
}