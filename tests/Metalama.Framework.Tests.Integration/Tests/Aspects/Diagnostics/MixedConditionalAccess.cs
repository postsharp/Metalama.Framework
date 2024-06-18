using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Diagnostics.MixedConditionalAccess;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        IFieldOrProperty? logger = null;

        if (logger?.Value != null)
        {
            logger.Value.ToString();
        }

        return null;
    }
}

internal class TargetCode
{
    [Aspect]
    private int Method( int a )
    {
        return a;
    }
}