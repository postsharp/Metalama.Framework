using Metalama.Testing.Framework;
using System.Collections.Generic;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Lambdas.RuntimeLinqSimpleLambda
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            var list = new List<int>();

            return list.Where(a => a > 0).Count();
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