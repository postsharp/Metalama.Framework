using System;
using Caravela.TestFramework;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples6
{
    class Aspect
    {
        [TestTemplate]
        dynamic Template()
        {
            (int a, byte b) left = meta.CompileTime((5, (byte)10));
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