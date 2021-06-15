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
        [Introduce]
        public event EventHandler? Event;
    }

    [TestOutput]
    [Introduction]
    internal class TargetClass
    {
    }
}
