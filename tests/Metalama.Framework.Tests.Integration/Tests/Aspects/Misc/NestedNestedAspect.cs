using System;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Misc.NestedNestedAspect;

using Metalama.Framework.Aspects;

[CompileTime]
public class Outer
{
    public class Inner
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
}

// <target>
class C
{
    [Outer.Inner.Log]
    void M()
    {
    }
}