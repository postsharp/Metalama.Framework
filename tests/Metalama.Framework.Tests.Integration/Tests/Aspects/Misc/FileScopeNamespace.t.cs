using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Misc.FileScopeNamespace;
#pragma warning disable CS0067

class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod() => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");

}
#pragma warning restore CS0067

class TargetCode
{
    [Aspect]
    int Method(int a)
{
    global::System.Console.WriteLine("Overridden.");
        return a;
}
}

