using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Misc.CompileTimeDelegate
{
    internal class Aspect : MethodAspect
    {
        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            // This is just to test that it builds.
            CompileTimeDelegate d = new( () => { } );
            d.Invoke();
        }
    }

    [CompileTimeOnly]
    internal delegate void CompileTimeDelegate();

    internal delegate void RunTimeOnlyDelegate();

    // <target>
    internal class TargetCode
    {
        [Aspect]
        private int Method( int a )
        {
            return a;
        }
    }
}