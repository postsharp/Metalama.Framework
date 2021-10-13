#pragma warning disable CS0067

using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Events.Declarative
{
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public event EventHandler? Event;
    }

    // <target>
    [Introduction]
    internal class TargetClass { }
}