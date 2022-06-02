using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using System;

#pragma warning disable CS0169
#pragma warning disable CS0414

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.Fields.PropertyTemplate_Field
{
    /*
     * Tests that overriding of fields with initializers correctly retains the initializer.
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
                Console.WriteLine($"This is the overridden setter.");
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override] 
        public int Field = 42;

        [Override]
        public static int StaticField = 42;

        static TargetClass()
        {
            StaticField = 27;
        }

        public TargetClass()
        {
            this.Field = 27;
        }
    }
}
