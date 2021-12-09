// @Skipped(Case for interface merge conflict resolution, not implemented.)

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.BaseClassImplicitInterfaceImplementation
{
    /*
     * When the base class of the target type implements the introduced interface implicitly, the transformed code should call the base class.
     */

    public interface IInterface
    {
        int InterfaceMethod();
    }

    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> aspectBuilder )
        {
            aspectBuilder.Advices.ImplementInterface(
                aspectBuilder.Target,
                (INamedType)aspectBuilder.Target.Compilation.TypeFactory.GetTypeByReflectionType( typeof(IInterface) ) );
        }

        [Introduce]
        public int InterfaceMethod()
        {
            Console.WriteLine( "This is introduced interface method." );

            return meta.Proceed();
        }
    }

    public class BaseClass : IInterface
    {
        public int InterfaceMethod()
        {
            return 27;
        }
    }

    // <target>
    [Introduction]
    public class TargetClass : BaseClass { }
}