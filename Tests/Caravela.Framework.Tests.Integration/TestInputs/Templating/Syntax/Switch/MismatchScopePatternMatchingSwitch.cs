using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.TestFramework;
using Caravela.Framework.Code;
using static Caravela.Framework.Aspects.TemplateContext;

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
                case IEnumerable<object> e when e.Count() == target.Parameters.Count:
                default:
                    Console.WriteLine("Default");
                    break;
            }

            return proceed();
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