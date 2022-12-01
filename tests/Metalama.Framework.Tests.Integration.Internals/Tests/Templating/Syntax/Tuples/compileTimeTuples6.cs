using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Tuples.CompileTimeTuples6
{
    class Aspect
    {
        [TestTemplate]
        dynamic? Template()
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