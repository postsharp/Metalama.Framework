#if ROSLYN_4_4_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.LanguageVersion.AuxiliaryTemplate_OldVersion;

public class TheAspect : TypeAspect
{
    [Introduce]
    private void M()
    {
        AuxiliaryTemplate();
        meta.InvokeTemplate( nameof(AuxiliaryTemplate) );
    }

    [Template]
    private void AuxiliaryTemplate()
    {
        Console.WriteLine( """aux""" );
    }
}

#endif