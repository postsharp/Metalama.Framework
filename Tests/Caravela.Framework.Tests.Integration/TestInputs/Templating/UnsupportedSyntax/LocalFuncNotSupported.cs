using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.LocalFuncNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            void LocalFunc(dynamic p)
            {
                Console.WriteLine(p.ToString());
            }

            dynamic result = proceed();

            LocalFunc(result);

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