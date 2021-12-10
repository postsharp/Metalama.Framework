using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssembly
{
    public class Aspect : TypeAspect, IInheritedAspect
    {
        [Introduce]
        public void Introduced() { }
    }
    
    [Aspect]
    public interface I
    {
        
    }
}