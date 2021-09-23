using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.MissingMembers
{
    /*
     * Case with missing interface members.
     */

    public interface IInterface
    {
        int Method();

        event EventHandler Event;

        int Property { get; set; }
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.Advices.ImplementInterface(aspectBuilder.Target, typeof(IInterface));            
        }
    }

    // <target>
    [Introduction]
    public class TargetClass
    {
    }
}
