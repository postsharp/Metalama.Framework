using System.Diagnostics;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32371;

public class LogAttribute : OverrideMethodAspect
{
       
    public override dynamic? OverrideMethod()
    {
        Debug.WriteLine($"Executing>>>>>>>>>>>>>>>>>>>>>>>>> {meta.Target.Method}.");

        return meta.Proceed();
    }
}

// <target>
public class C
{
    [Log]
    void M() { }
}