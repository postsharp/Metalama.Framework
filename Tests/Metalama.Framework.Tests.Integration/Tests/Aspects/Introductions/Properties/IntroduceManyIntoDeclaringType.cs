using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Introductions.Properties.IntroduceManyIntoDeclaringType
{
    internal class Aspect : MethodAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Ignore )]
        private string? Property { get; set; }
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