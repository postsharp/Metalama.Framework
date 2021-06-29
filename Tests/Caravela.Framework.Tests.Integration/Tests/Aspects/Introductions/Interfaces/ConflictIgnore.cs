using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ConflictIgnore
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface
    {
        int InterfaceMethod();
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.ImplementInterface(
                aspectBuilder.TargetDeclaration,
                typeof(IInterface),
                conflictBehavior: ConflictBehavior.Ignore);
        }

        [InterfaceMember]
        public int InterfaceMethod()
        {
            Console.WriteLine("This is introduced interface method.");
            return meta.Proceed();
        }
    }

    // <target>
    [Introduction]
    public class TargetClass : IInterface
    {
        int IInterface.InterfaceMethod()
        {
            Console.WriteLine("This is the original implementation.");
            return 42;
        }
    }
}
