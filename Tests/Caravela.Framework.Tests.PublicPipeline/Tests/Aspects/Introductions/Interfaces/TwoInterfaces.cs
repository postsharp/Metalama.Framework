using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TwoInterfaces
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface1
    {
    }
    public interface IInterface2
    {
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.ImplementInterface(aspectBuilder.Target, typeof(IInterface1));
            aspectBuilder.AdviceFactory.ImplementInterface(aspectBuilder.Target, typeof(IInterface2));
        }
    }

    // <target>
    [Introduction]
    public class TargetClass
    {
    }
}
