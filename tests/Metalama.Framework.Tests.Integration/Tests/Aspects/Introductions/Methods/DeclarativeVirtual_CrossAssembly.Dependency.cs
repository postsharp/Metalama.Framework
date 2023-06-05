using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8618, CS8602

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Methods.DeclarativeVirtual_CrossAssembly;

public abstract class IntroductionAttribute : TypeAspect
{
    [Introduce(IsVirtual = false)]
    public virtual void VirtualOverriddenIntroduction()
    {
        Console.WriteLine("Base template (wrong).");
    }

    [Introduce(IsVirtual = false)]
    public virtual void VirtualIntroduction()
    {
        Console.WriteLine("Base template (expected).");
    }
}