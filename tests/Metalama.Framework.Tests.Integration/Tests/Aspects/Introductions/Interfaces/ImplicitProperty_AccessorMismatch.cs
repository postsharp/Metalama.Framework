using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.ImplicitProperty_AccessorMismatch
{
    /*
     * Error when accessors of implicit property interface member don't match the interface.
     */

    public interface IInterface
    {
        int TemplateWithInit { set; }
        int TemplateWithoutInit { init; }
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advice.ImplementInterface( aspectBuilder.Target, typeof(IInterface) );
        }

        [Introduce]
        public int TemplateWithInit
        {
            init
            {
            }
        }

        [Introduce]
        public int TemplateWithoutInit
        {
            set
            {
            }
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}