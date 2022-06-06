using Metalama.Framework.Aspects;
using Metalama.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.ExplicitInterfaceImplementation
{
    internal class TestAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("Overridden code.");
            return meta.Proceed();
        }
    }

    public interface Interface
    {
        void Foo();

        void Bar<T>();
    }

    // <target>
    public class Target : Interface
    {
        [Test]
        void Interface.Foo()
        {
            Console.WriteLine("Original");
        }

        [Test]
        void Interface.Bar<T>()
        {
            Console.WriteLine("Original");
        }
    }
}
