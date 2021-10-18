using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.Integration.Tests.Aspects.Inheritance.CrossAssembly
{
    public class Aspect : TypeAspect
    {
        public override void BuildAspectClass( IAspectClassBuilder builder )
        {
            builder.IsInherited = true;
            base.BuildAspectClass( builder );
        }

        [Introduce]
        public void Introduced() { }
    }
    
    [Aspect]
    public interface I
    {
        
    }
}