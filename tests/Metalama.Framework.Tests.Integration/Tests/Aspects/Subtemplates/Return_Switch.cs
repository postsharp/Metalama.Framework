using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Return_Switch;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        CalledTemplate(0);
        CalledTemplate(1);
        return default;
    }

    [Template]
    private void CalledTemplate([CompileTime] int i)
    {
        switch (i)
        {
            case 1:
                return;
        }
    }
}

// <target>
internal class TargetCode
{
    [Aspect]
    private void Method()
    {
    }
}