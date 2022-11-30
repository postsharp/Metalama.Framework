using System;
using System.Linq;
using System.Collections.Generic;
using Metalama.Testing.AspectTesting;
using Metalama.Framework.Code;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Switch.MismatchScopePatternMatchingSwitch
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var o = new object();

            switch (o)
            {
                case IParameter p:
                    Console.WriteLine("0");
                    break;
                case IEnumerable<object> e when e.Count() == meta.Target.Parameters.Count:
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