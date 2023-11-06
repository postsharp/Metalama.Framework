using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.Template_Method_OldVersion;

public class TheAspect : TypeAspect
{
    [Introduce]
    public void Method()
    {
        Console.WriteLine("""method""");
    }
}