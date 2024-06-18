using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.New.RunTimeNew
{
    internal class Aspect
    {
        [TestTemplate]
        private dynamic? Template()
        {
            var o = new object();
            Console.WriteLine( o.GetType().ToString() );

            return meta.Proceed();
        }
    }

    internal class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}