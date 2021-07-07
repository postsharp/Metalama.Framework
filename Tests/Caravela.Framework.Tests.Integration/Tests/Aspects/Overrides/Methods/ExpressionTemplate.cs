using Caravela.Framework.Aspects;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.ExpressionTemplate
{
    internal class TestAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
            => default;
    }

    // <target>
    public class Target
    {
        [Test]
        public int Foo()
        {
            Console.WriteLine("Original");
            return 42;
        }

        [Test]
        public void Bar()
        {
            Console.WriteLine("Original");
        }
    }
}
