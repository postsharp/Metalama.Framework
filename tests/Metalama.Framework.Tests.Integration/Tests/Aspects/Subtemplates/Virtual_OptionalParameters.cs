using System;
using System.Threading.Tasks;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Virtual_OptionalParameters;

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("regular template");

        CalledTemplate(1, 2);
        CalledTemplate(1);
        CalledTemplate();

        return meta.Proceed();
    }

    [Template]
    protected virtual void CalledTemplate(int i = -1, [CompileTime] int j = -2)
    {
        Console.WriteLine($"called template i={i} j={j}");
    }
}

// <target>
class TargetCode
{
    [Aspect]
    void Method()
    {
    }
}