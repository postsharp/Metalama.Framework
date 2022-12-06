using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp11.RawLiteral_TargetCode;

public class TheAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        Console.WriteLine("Oops");
    
        return meta.Proceed();
    }
}

public class C
{
     // <target>
    [TheAspect]
    private void M()
    {
        string longMessage = """
    This is a long message.
    It has several lines.
        Some are indented
                more than others.
    Some should start at the first column.
    Some have "quoted text" in them.
    """;

        Console.WriteLine( longMessage );

    }
}