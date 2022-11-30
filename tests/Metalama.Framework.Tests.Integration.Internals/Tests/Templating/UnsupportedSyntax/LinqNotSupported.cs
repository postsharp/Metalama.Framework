using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Testing.AspectTesting;

namespace Metalama.Framework.Tests.Integration.Templating.UnsupportedSyntax.LinqNotSupported
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            dynamic? result = meta.Proceed();

#pragma warning disable CS0618
            IEnumerable<int> list = from i in new int[] { 1, 2, 3 } select i * i;
#pragma warning restore CS0618
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