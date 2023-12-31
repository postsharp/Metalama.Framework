using System;
using System.Linq;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Switch.RuntimeatternMatchingSwitch
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var o = new object();

            switch (o)
            {
                case IEnumerable<object> a when a.Any():
                    Console.WriteLine("0");
                    break;
                default:
                    Console.WriteLine("Default");
                    break;
            }

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