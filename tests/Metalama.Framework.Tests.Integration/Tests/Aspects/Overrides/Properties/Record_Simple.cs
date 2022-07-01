using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using System;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Properties.Record_ImplicitProperties
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
        [Override]
        public int Property;

        [Override]
        public static int StaticProperty;

        public TargetRecord( int property, int staticProperty)
        {
            Property = property;
            StaticProperty = staticProperty;
        }
    }
}