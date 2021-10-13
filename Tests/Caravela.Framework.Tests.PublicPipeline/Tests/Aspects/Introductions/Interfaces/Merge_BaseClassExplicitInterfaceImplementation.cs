// @Skipped(Case for interface merge conflict resolution, not implemented.)

using System;
using Caravela.Framework.Aspects;
using Caravela.Framework.Code;

namespace Caravela.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.BaseClassExplicitInterfaceImplementation
{
    /*
     * When the base class of the target type implements the introduced interface explicitly, an error should be produced, because C# does not allow calling
     * base class' explicit interface implementation.
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
        int IInterface.InterfaceMethod()
        {
            Console.WriteLine( "This is original interface method." );

            return 27;
        }
    }

    // <target>
    [Introduction]
    public class TargetClass : BaseClass { }
}