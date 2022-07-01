﻿using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using System;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Target_Record_Simple
{
    /*
     * Tests that a basic case of override property with property template works.
     */

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
                Console.WriteLine("This is the overridden setter.");
                meta.Proceed();
            }
        }
    }

    // <target>
    internal record TargetRecord
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

        private static int _staticField;

        [Override]
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
    }
}