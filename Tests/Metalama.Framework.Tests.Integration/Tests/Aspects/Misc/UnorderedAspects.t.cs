// Warning LAMA0035 on ``: `The aspect layers 'Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.UnorderedAspects.Aspect1' and 'Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.UnorderedAspects.Aspect2' are not strongly ordered. Add an [assembly: AspectOrderAttribute(...)] attribute to specify the order relationship between these two layers, otherwise the compilation will be non-deterministic.`
using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Misc.UnorderedAspects;
#pragma warning disable CS0067

public class Aspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}
#pragma warning restore CS0067
#pragma warning disable CS0067

public class Aspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}
#pragma warning restore CS0067


public class T
{
    [Aspect1, Aspect2]
    public void M() {     global::System.Console.WriteLine("Aspect2");
        return;
}
}
