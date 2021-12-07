using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.IfTests.IfParametersCount
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            bool b = meta.CompileTime(false);

            if (meta.Target.Parameters.Count > 0)
            {
                b = true;
            }
            else
            {
                b = false;
            }

            Console.WriteLine(b);

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