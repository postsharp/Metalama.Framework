using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples6
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            (int a, byte b) left = (5, 10);
            Console.WriteLine(left.a);

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