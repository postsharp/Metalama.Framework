using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.TestFramework;
using Caravela.Framework.Code;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.MismatchScopePatternMatchingSwitch
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var o = new object();

            switch (o)
            {
                case IParameter p:
                    Console.WriteLine("0");
                    break;
                case IEnumerable<object> e when e.Count() == meta.Parameters.Count:
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