using System;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Misc.NestedAspect;

using Metalama.Framework.Aspects;

[CompileTime]
public class Outer
{
    public class LogAttribute : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine(meta.Target.Method.ToDisplayString() + " started.");
            return meta.Proceed();
        }
    }
}

// <target>
class C
{
    [Outer.Log]
    void M()
    {
    }
}