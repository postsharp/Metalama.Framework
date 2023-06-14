using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8618, CS8602

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Properties.DeclarativeVirtual_CrossAssembly;

public class InheritedIntroductionAttribute : IntroductionAttribute
{
    public override int VirtualOverriddenIntroduction
    {
        get
        {
            Console.WriteLine("Base template (expected).");
            return 42;
        }
        set
        {
            Console.WriteLine("Base template (expected).");
        }
    }
}

// <target>
[InheritedIntroduction]
internal class TargetClass { }