﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;
using System;


#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Auto_GetOnly
{
    /*
     * Tests a single OverrideProperty aspect on get-only auto properties, including introduced get-only auto properties.
     */

    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        {
            get
            {
                Console.WriteLine($"This is the overridden getter.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine($"This is the overridden setter.");
                meta.Proceed();
            }
        }
    }

    internal abstract class BaseClass
    {
        public abstract int AbstractBaseProperty { get; }

        public abstract int AbstractBaseInitializerProperty { get; }

        public virtual int VirtualBaseProperty => 0;

        public virtual int VirtualBaseInitializerProperty => 0;
    }

    // <target>
    internal class TargetClass : BaseClass
    {
        [Override]
        public int Property { get; }

        [Override]
        public static int StaticProperty { get; }

        [Override]
        public int InitializerProperty { get; } = 42;

        [Override]
        public static int StaticInitializerProperty { get; } = 42;

        [Override]
        public override int AbstractBaseProperty { get; }

        [Override]
        public override int AbstractBaseInitializerProperty { get; } = 42;

        [Override]
        public override int VirtualBaseProperty { get; }

        [Override]
        public override int VirtualBaseInitializerProperty { get; } = 42;

        public TargetClass()
        {
            this.Property = 27;
            this.InitializerProperty = 27;
            this.AbstractBaseProperty = 27;
            this.AbstractBaseInitializerProperty = 27;
            this.VirtualBaseProperty = 27;
            this.VirtualBaseInitializerProperty = 27;
        }

        static TargetClass()
        {
            StaticProperty = 27;
            StaticInitializerProperty = 27;
        }
    }
}
