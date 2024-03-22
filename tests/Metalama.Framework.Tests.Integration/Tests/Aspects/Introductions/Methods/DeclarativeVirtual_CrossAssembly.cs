using System;

#pragma warning disable CS8618, CS8602

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.DeclarativeVirtual_CrossAssembly;

public class InheritedIntroductionAttribute : IntroductionAttribute
{
    public override void VirtualOverriddenIntroduction()
    {
        Console.WriteLine("Override template (expected).");
    }
}

// <target>
[InheritedIntroduction]
internal class TargetClass { }