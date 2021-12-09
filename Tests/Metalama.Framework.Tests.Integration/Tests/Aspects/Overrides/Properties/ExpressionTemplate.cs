using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.ExpressionTemplate
{
    internal class TestAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get => default;
            set => Console.WriteLine("nothing");
        }
    }

    // <target>
    public class Target
    {
        [Test]
        public int A
        {
            get
            {
                Console.WriteLine("Original");
                return 42;
            }
            set
            {
                Console.WriteLine("Original");
            }
        }
    }
}
