using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple;

[assembly: AspectOrder( typeof(FirstOverrideAttribute), typeof(SecondOverrideAttribute), typeof(IntroduceAndOverrideAttribute) )]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Multiple
{
    /*
     * Tests that multiple aspects overriding the same property produce correct code.
     */

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

    public class IntroduceAndOverrideAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.With(x => x.FieldsAndProperties).AddAspect(x => new FirstOverrideAttribute());
            builder.With(x => x.FieldsAndProperties).AddAspect(x => new SecondOverrideAttribute());
        }

        [Introduce]
        public int IntroducedField;

        [Introduce]
        public readonly int IntroducedReadOnlyField;
    }

    // <target>
    [IntroduceAndOverride]
    internal class TargetClass
    {
        private int _field;

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

        public int ExpressionBodiedProperty => 42;

        public int AutoProperty { get; set; }

        public int GetOnlyAutoProperty { get; }

        public int InitializerAutoProperty { get; set; } = 42;

        public TargetClass()
        {
            this.GetOnlyAutoProperty = 42;
        }
    }
}