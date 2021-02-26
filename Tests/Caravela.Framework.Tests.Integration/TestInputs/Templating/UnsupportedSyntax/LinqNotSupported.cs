using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Tests.Integration.Templating;
using static Caravela.Framework.Aspects.TemplateContext;

namespace Caravela.Framework.IntegrationTests.Templating.UnsupportedSyntax.LinqNotSupported
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            dynamic result = proceed();

            IEnumerable<int> list = from i in new int[] { 1, 2, 3 } select i * i;
            if (result == null)
            {
                result =
                    from i in list
                    from i2 in list
                    let ii = i * i
                    where true
                    orderby i, i2 descending
                    join j in list on i equals j
                    join j2 in list on i equals j2 into g
                    group i by i2;
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