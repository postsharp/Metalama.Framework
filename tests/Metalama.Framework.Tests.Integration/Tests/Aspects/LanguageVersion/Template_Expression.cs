using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.Template_Expression;

public class TheAspect : TypeAspect
{
    [Introduce]
    public string Property => """
        This is a long message.
        It has several lines.
            Some are indented
                    more than others.
        Some should start at the first column.
        Some have "quoted text" in them.
        """;
}

// <target>
[TheAspect]
class Target
{
}