using System;
using Metalama.Framework.Aspects;

public class Aspect2 : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        var version = meta.CompileTime( typeof(IAspectBuilder).Assembly.GetName() );
        Console.WriteLine($"Aspect2 on {meta.Target.Method} compiled with {version}");
        return meta.Proceed();
    }
}