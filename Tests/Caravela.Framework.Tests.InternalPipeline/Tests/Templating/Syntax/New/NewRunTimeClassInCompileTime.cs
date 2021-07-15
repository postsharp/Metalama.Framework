using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.New.NewRunTimeClassInCompileTime
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
