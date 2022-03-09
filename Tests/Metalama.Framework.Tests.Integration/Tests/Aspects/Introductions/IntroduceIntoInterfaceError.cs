using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Aspects.Introductions.IntroduceIntoInterfaceError
{
    internal class Aspect : TypeAspect
    {
        [Introduce]
        private void NewMethod() { }
    }

    [Aspect]
    internal interface TargetCode { }
}