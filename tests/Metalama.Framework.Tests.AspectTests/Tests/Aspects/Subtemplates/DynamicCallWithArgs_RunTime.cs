using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.AspectTests.Tests.Aspects.Subtemplates.DynamicCallWithArgs_RunTime;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        meta.InvokeTemplate( nameof(CalledTemplate), args: new { a = 42 } );

        return default;
    }

    [Template]
    private void CalledTemplate( int a )
    {
        Console.WriteLine( $"called template a={a}" );
    }
}

internal class TargetCode
{
    // <target>
    [Aspect]
    private void Method() { }
}