using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Fields.Target_ReadOnlyStruct_Simple
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
    internal readonly struct TargetStruct
    {
        [Test]
        public readonly int Field;

        [Test]
        public static int StaticField;

        [Test]
        public static readonly int StaticReadOnlyField;
    }
}
