using Metalama.Framework.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Formatting.UserCodeBetweenGeneratedCode
{
    internal class LogAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine($"Invoking {meta.Target.Method.ToDisplayString()}");

            return meta.Proceed();
        }
    }

    internal class Foo
    {
        [Log]
        public void Method()
        {
            Console.WriteLine("InstanceMethod");
        }

        [Log]
        public void ExpressionMethod() => Console.WriteLine("InstanceMethod");
    }
}
