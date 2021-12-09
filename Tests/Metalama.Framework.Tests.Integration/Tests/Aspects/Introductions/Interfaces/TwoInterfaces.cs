using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.TwoInterfaces
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface1 { }

    public interface IInterface2 { }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advices.ImplementInterface( aspectBuilder.Target, typeof(IInterface1) );
            aspectBuilder.Advices.ImplementInterface( aspectBuilder.Target, typeof(IInterface2) );
        }
    }

    // <target>
    [Introduction]
    public class TargetClass { }
}