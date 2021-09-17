using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.TestFramework;
using System;

#pragma warning disable CS0067

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.BaseClass
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface
    {
    }

    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> aspectBuilder)
        {
            aspectBuilder.AdviceFactory.ImplementInterface(aspectBuilder.Target, typeof(IInterface));
        }
    }

    public class BaseClass
    {
    }

    // <target>
    [Introduction]
    public class TargetClass : BaseClass
    {
    }
}
