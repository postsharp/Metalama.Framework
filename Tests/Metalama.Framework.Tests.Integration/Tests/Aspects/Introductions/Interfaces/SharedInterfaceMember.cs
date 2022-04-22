//@Skipped(30228)

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.SharedInterfaceMember
{
    /*
     * Single interface member matches two interfaces.
     */

    public interface IInterface1 
    {
        void Foo();
    }

    public interface IInterface2 
    {
        void Foo();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advices.ImplementInterface(aspectBuilder.Target, typeof(IInterface1));
            aspectBuilder.Advices.ImplementInterface(aspectBuilder.Target, typeof(IInterface2));
        }

        [InterfaceMember]
        public void Foo()
        {
            Console.WriteLine("Interface member.");
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}
