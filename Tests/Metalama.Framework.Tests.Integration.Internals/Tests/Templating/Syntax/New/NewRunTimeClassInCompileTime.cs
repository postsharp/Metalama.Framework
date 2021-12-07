using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.New.NewRunTimeClassInCompileTime
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            var o = meta.CompileTime(new TargetCode());
            Console.WriteLine(o.GetType().ToString());

            return meta.Proceed();
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
