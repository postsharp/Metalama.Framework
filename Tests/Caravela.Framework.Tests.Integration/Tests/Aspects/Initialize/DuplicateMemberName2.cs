using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Initialize.DuplicateMemberName2
{
    // Error: the base class already defines a template and this is not an override.

    internal class Aspect1 : MethodAspect
    {
        [Template]
        public void Template() { }
    }

    internal class Aspect2 : Aspect1
    {
        [Template]
        public new void Template() { }
    }

    // <target>
    internal class Target { }
}