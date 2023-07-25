using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Diagnostics.VarError;

public class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var varDeclaration = meta.RunTime(meta.Target.Declaration);

        return meta.Proceed();
    }
}