using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Inheritance_BaseAfterDerived
{
    /*
     * Tests that when a single aspect introduces a base interface after the derived interface, an error is emitted.
     */

    public interface IBaseInterface
    {
        int Foo();
    }

    public interface IDerivedInterface : IBaseInterface
    {
        int Bar();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IDerivedInterface), tags: new { Source = "Derived" });
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IBaseInterface), OverrideStrategy.Ignore, tags: new { Source = "Base" });
        }

        [InterfaceMember]
        public int Foo()
        {
            Console.WriteLine($"This is introduced interface member by {meta.Tags["Source"]} (should be Derived).");

            return meta.Proceed();
        }

        [InterfaceMember]
        public int Bar()
        {
            Console.WriteLine($"This is introduced interface member by {meta.Tags["Source"]} (should be Derived).");

            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}