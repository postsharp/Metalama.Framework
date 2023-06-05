using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS8618, CS8602

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Events.DeclarativeVirtual_CrossAssembly;

public abstract class IntroductionAttribute : TypeAspect
{
    [Introduce(IsVirtual = false)]
    public virtual event System.EventHandler VirtualOverriddenIntroduction
    {
        add
        {
            Console.WriteLine("Base template (wrong).");
        }
        remove
        {
            Console.WriteLine("Base template (wrong).");
        }
    }

    [Introduce(IsVirtual = false)]
    public virtual event System.EventHandler VirtualIntroduction
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