using System.Collections.Generic;
using System.Linq;
using Caravela.Framework.Aspects;
using Caravela.TestFramework;

namespace Caravela.Framework.Tests.Integration.Templating.UnsupportedSyntax.LinqNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            dynamic? result = meta.Proceed();

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