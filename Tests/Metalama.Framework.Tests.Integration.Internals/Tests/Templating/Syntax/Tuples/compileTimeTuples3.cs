using System;
using Metalama.TestFramework;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples3
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
        {
            (int, string) anonymT = meta.CompileTime((4, ""));
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