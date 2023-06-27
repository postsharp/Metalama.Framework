using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.ForTests.CompileTimeFor;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        for (int i = meta.CompileTime(0); i < 3; i++)
        {
            Console.WriteLine(i);
        }

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