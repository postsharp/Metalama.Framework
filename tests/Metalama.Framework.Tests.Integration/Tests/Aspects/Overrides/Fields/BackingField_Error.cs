using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.BackingField_Error
{
    public class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty 
        {
            get
            {
                Console.WriteLine("This is aspect code.");
                return meta.Proceed();
            }
            set
            {
                Console.WriteLine("This is aspect code.");
                meta.Proceed();
            }
        }
    }

    // <target>
    internal class TargetClass
    {
        [field:Test]
        public int AutoProperty { get; set; }
    }
}
