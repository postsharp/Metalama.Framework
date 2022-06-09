using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.Template_NameOfProperty
{
    internal class TestAttribute : OverrideMethodAspect
    {
        public int MyProperty { get; set; }

        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(nameof(TestAttribute.MyProperty));
            return default;
        }
    }

    // <target>
    public class Target
    {
        [Test]
        public void VoidMethod()
        {
            Console.WriteLine("Original");
        }

        [Test]
        public int Method()
        {
            Console.WriteLine("Original");
            return 42;
        }

        [Test]
        public T? Method<T>()
        {
            Console.WriteLine("Original");
            return default;
        }
    }
}
