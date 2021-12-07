using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.BaseClass
{
    /*
     * Simple case with implicit interface members.
     */

    public interface IInterface { }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advices.ImplementInterface( aspectBuilder.Target, typeof(IInterface) );
        }
    }

    public class BaseClass { }

    // <target>
    [Introduction]
    public class TargetClass : BaseClass { }
}