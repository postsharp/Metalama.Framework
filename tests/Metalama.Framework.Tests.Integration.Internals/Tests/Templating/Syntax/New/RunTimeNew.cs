using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.New.RunTimeNew
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var o = new object();
            Console.WriteLine(o.GetType().ToString());
            
            return meta.Proceed();
        }
    }

    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}