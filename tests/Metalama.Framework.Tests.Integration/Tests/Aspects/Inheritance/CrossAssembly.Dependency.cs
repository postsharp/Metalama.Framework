using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssembly
{
    [Inheritable]
    public class Aspect : TypeAspect
    {
        [Introduce]
        public void Introduced() { }
    }

    [Aspect]
    public interface I { }

    public interface J : I { }
}