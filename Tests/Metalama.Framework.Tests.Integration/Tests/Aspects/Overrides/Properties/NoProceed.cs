﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.NoProceed
{
    /*
     * Tests a template without meta.Proceed.
     */

    public class OverrideAttribute : FieldOrPropertyAspect
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
                Console.WriteLine("Override.");
                return default;
            }

            set
            {
                Console.WriteLine("Override.");
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        private int _field;

        [Override]
        public int Property
        {
            get
            {
                return this._field;
            }

            set
            {
                this._field = value;
            }
        }

        private static int _staticfield;

        [Override]
        public static int StaticProperty
        {
            get
            {
                return _staticfield;
            }

            set
            {
                _staticfield = value;
            }
        }

        [Override]
        public int AutoProperty { get; set; }

        [Override]
        public int GetAutoProperty { get; }

        [Override]
        public int InitializerAutoProperty { get; set; } = 42;
    }
}