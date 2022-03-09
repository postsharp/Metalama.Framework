#pragma warning disable CS0067

using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Introductions.Events.IntroduceManyIntoDeclaringType
{
    internal class Aspect : MethodAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Ignore )]
        private event Action? Event;
    }

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private void M() { }

        [Aspect]
        private void M2() { }
    }
}