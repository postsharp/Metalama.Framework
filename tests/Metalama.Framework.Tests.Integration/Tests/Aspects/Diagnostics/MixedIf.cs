using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Diagnostics.MixedIf;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        IFieldOrProperty? logger = null;

        if (logger != null && logger.Value != null)
        {
            logger!.Value!.ToString();
        }

        return null;
    }
}

class TargetCode
{
    [Aspect]
    int Method(int a)
    {
        return a;
    }
}