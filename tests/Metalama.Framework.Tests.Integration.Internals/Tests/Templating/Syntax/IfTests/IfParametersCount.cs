using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

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