using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TargetType_Struct
{
    /*
     * Tests that target having a base class does not interfere with interface introduction.
     */

    public class IntroduceAspectAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface) );
        }

        [InterfaceMember]
        public void IntroducedMethod()
        {
            Console.WriteLine("Introduced interface member");
        }
    }

    public interface IInterface
    {
        void IntroducedMethod();
    }

    // <target>
    [IntroduceAspect]
    public struct TargetStruct 
    {
        public void ExistingMethod()
        {
            Console.WriteLine("Original struct member");
        }
    }
}