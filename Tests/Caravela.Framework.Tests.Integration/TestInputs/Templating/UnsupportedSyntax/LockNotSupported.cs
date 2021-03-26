using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.LockNotSupported
{
    class Aspect
    {
        private static readonly object o = new object();

        [TestTemplate]
        dynamic Template()
        {
            dynamic result;
            lock (o)
            {
                result = proceed();
            }
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