#if TESTOPTIONS
// @Skipped(Derived type support for introduced types)
#endif

using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Inheritance.IntroducedDerivedType
{
    [Inheritable]
    internal class Aspect : TypeAspect
    {
        [Introduce( WhenExists = OverrideStrategy.Override)]
        private dynamic? Foo()
        {
            Console.WriteLine( "Introduced!" );

            return meta.Proceed();
        }
    }

    internal class Introduction : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceType(builder.Target, "Derived", TypeKind.Class, b => { b.BaseType = builder.Target.NestedTypes.Single(); });
        }
    }

    // <target>
    [Introduction]
    internal class Targets
    {
        [Aspect]
        private class BaseType { }
    }
}