using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.TestFramework;

namespace Metalama.Framework.Tests.Integration.Templating.Dynamic.Parse
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var exp = meta.ParseExpression("1 + 1 / System.Math.Pi ");
            var x = exp.Value;
            return default;
        }
    }

    // <target>
    class TargetCode
    {
        int Method(int a)
        {
            return a;
        }
    }
}