﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable_Multiple;

[assembly: AspectOrder(typeof(FirstOverrideAttribute), typeof(SecondOverrideAttribute))]

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Uninlineable_Multiple
{
    /*
     * Tests that two overrides with an uninlineable expanded template produces correct code.
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
                _ = meta.Proceed();
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("First override.");
                meta.Proceed();
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
                _ = meta.Proceed();
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("Second override.");
                meta.Proceed();
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

        [FirstOverride]
        [SecondOverride]
        public int AutoGetOnlyProperty { get; }
    }
}