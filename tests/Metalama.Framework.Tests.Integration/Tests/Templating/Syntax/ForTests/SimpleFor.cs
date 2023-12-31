using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForTests.SimpleFor
{
    [CompileTime]
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {

            for (int i = 0; i < 3; i++)
            {
                try
                {
                    return meta.Proceed();
                }
                catch
                {
                }
            }

            throw new Exception();
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