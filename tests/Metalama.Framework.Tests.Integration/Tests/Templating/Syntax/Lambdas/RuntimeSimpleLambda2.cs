using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.RuntimeSimpleLambda2
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            Action<object?> action = a => Console.WriteLine( a?.ToString() );

            var result = meta.Proceed();

            action( result );

            return result;
        }
    }

    internal class TargetCode
    {
        private int Method( int a, int b )
        {
            return a + b;
        }
    }
}