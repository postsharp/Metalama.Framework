#if ROSLYN_4_4_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.AuxiliaryTemplate_OldVersion;

public class TheAspect : TypeAspect
{
    [Introduce]
    void M()
    {
        AuxiliaryTemplate();
        meta.InvokeTemplate(nameof(AuxiliaryTemplate));
    }

    [Template]
    void AuxiliaryTemplate()
    {
        Console.WriteLine("""aux""");
    }
}

#endif