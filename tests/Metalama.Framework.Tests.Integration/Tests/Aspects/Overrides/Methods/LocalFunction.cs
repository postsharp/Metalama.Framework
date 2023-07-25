using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.LocalFunction
{
    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine("This is overridden method.");

            return Quz();

            int Quz()
            {
                var x = meta.Proceed();
                return x + 1;
            }
        }
    }

    // <target>
    internal class TargetClass 
    {
        [Override]
        public int Foo(int x)
        {
            return Bar(Bar(x));

            int Bar( int x )
            {
                return x + 1;
            }
        }
    }
}