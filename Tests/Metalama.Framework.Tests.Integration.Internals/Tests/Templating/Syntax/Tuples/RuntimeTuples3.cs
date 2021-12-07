using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.RunTimeTuples3
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            (int, string) anonymT = (4, "");
            Console.WriteLine(anonymT.Item1);
            
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