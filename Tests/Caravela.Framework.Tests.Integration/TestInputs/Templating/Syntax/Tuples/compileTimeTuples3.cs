using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples3
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
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