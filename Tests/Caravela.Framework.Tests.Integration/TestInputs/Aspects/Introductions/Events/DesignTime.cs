// @DesignTime

#pragma warning disable CS0067

using Caravela.Framework.Aspects;
using Caravela.Framework.Code;
using Caravela.Framework.Eligibility;
using Caravela.TestFramework;
using System;
using System.Collections.Generic;
using System.Text;

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

    [TestOutput]
    [Introduction]
    internal partial class TargetClass
    {
    }
}
