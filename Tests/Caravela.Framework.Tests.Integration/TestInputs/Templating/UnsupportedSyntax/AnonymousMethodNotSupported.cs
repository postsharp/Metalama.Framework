using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.AnonymousMethodNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            Action<object> action =
            delegate (object p)
            {
                Console.WriteLine(p.ToString());
            };

            dynamic result = proceed();

            action(result);

            return result;
        }
    }

    class TargetCode
    {
        int Method(int a, int b)
        {
            return a + b;
        }
    }
}