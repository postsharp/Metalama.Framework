using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples6
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