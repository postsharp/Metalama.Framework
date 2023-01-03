using System;
using Metalama.Framework.Aspects;

public class Aspect1 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var version = meta.CompileTime( typeof(IAspectBuilder).Assembly.GetName() );
        Console.WriteLine($"Aspect1 on {meta.Target.Method} compiled with {version}");
        return meta.Proceed();
    }
}