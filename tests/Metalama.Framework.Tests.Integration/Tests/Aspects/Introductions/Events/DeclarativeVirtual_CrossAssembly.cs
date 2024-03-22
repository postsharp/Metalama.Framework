using System;

#pragma warning disable CS8618, CS8602

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Events.DeclarativeVirtual_CrossAssembly;

public class InheritedIntroductionAttribute : IntroductionAttribute
{
    public override event System.EventHandler VirtualOverriddenIntroduction
    {
        add
        {
            Console.WriteLine("Base template (expected).");
        }
        remove
        {
            Console.WriteLine("Base template (expected).");
        }
    }
}

// <target>
[InheritedIntroduction]
internal class TargetClass { }