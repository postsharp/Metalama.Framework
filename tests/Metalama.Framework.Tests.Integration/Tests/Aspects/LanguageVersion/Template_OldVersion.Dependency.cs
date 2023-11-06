using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.Template_OldVersion;

public class TheAspect : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        string longMessage = """
    This is a long message.
    It has several lines.
        Some are indented
                more than others.
    Some should start at the first column.
    Some have "quoted text" in them.
    """;

        Console.WriteLine(longMessage);

        return meta.Proceed();
    }
}