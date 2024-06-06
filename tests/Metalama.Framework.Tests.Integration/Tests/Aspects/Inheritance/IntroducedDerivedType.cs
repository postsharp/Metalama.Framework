using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Inheritance.IntroducedDerivedType
{
    [Inheritable]
    internal class Aspect : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override, IsVirtual = true )]
        public int Foo()
        {
            Console.WriteLine( "Introduced!" );

            return meta.Proceed();
        }
    }

    internal class Introduction : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.IntroduceClass(
                builder.Target,
                "IntroducedDerived",
                b => { b.BaseType = builder.Target.Types.OfName( "BaseType" ).Single(); } );
        }
    }

    // <target>
    [Introduction]
    public class Targets
    {
        [Aspect]
        public class BaseType { }

        public class ManualDerived : BaseType { }
    }
}