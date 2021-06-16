#pragma warning disable CS0067

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.Declarative
{
    public class IntroductionAttribute : Attribute, IAspect<INamedType>
    {
        public void BuildAspect(IAspectBuilder<INamedType> builder)
        {
        }

        public void BuildEligibility(IEligibilityBuilder<INamedType> builder) 
        { 
        }

        [Introduce]
        public event EventHandler? Event;
    }

    // <target>
    [Introduction]
    internal class TargetClass
    {
    }
}
