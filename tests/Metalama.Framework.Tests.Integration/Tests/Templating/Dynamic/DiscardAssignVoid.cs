using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.DiscardAssignVoid
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            _ = meta.Proceed();

            return default;
        }
    }

    internal class TargetCode
    {
        private void Method( int a )
        {
            Console.WriteLine( "Hello, world." );
        }
    }
}