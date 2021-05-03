using System;
using System.Linq;
using System.Collections.Generic;
using Caravela.TestFramework;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Switch.RuntimeatternMatchingSwitch
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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