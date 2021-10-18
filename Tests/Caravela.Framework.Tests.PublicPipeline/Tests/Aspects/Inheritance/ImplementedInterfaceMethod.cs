using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Inheritance.ImplementedInterfaceMethod
{
    internal class Aspect : OverrideMethodAspect
    {
        public override void BuildAspectClass( IAspectClassBuilder builder )
        {
            builder.IsInherited = true;
        }

        public override dynamic OverrideMethod()
        {
            Console.WriteLine( "Overridden!" );

            return meta.Proceed();
        }
    }

    // <target>
    internal class Targets
    {
        private interface I
        {
            [Aspect]
            void M();
        }

        private class C: I
        {
            public void M() { }
        }
    }
}