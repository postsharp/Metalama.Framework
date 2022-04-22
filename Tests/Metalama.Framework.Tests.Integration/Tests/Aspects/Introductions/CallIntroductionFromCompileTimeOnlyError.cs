using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.CallIntroductionFromCompileTimeOnlyError;

public class Aspect : TypeAspect
{
    [Introduce]
    private int f;

    [CompileTime]
    public void CompileTimeMethod()
    {
        f = 5;
    }
}

[Aspect]
internal class T { }