using Metalama.Framework.Aspects;
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
                Console.WriteLine( "This is the overridden getter." );

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine( "This is the overridden setter." );
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public int Property { get; }

        [Override]
        public static int StaticProperty { get; }

        [Override]
        public int InitializerProperty { get; } = 42;

        [Override]
        public static int StaticInitializerProperty { get; } = 42;

        public TargetClass()
        {
            Property = 27;
            InitializerProperty = 27;
        }

        static TargetClass()
        {
            StaticProperty = 27;
            StaticInitializerProperty = 27;
        }
    }
}