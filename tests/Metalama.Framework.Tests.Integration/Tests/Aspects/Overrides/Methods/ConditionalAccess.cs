using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.ConditionalAccess
{
    // Tests single OverrideMethod aspect with trivial template on methods with trivial bodies.

    public class OverrideAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "This is the overriding method." );
            var x = meta.This;

            return meta.Target.Method.Invokers.ConditionalBase?.Invoke( x );
        }
    }

    // <target>
    internal class TargetClass
    {
        [Override]
        public void TargetMethod_Void()
        {
            Console.WriteLine( "This is the original method." );
        }

        [Override]
        public int? TargetMethod_Int()
        {
            Console.WriteLine( "This is the original method." );

            return 42;
        }
    }
}