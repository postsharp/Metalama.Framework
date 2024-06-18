using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Return_If;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        CalledTemplate( false );
        CalledTemplate( true );

        return default;
    }

    [Template]
    private void CalledTemplate( [CompileTime] bool shouldReturn )
    {
        Console.WriteLine( $"Shold return? {shouldReturn}" );

        if (shouldReturn)
        {
            return;
        }
    }
}

// <target>
internal class TargetCode
{
    [Aspect]
    private void Method() { }
}