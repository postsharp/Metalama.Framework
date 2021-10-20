using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Inheritance.ImplementedGenericInterfaceMethod
{
    internal class Aspect : OverrideMethodAspect
    {
        public override void BuildAspectClass( IAspectClassBuilder builder )
        {
            builder.IsInherited = true;
        }

        public override dynamic? OverrideMethod()
        {
            Console.WriteLine( "Overridden!" );

            return meta.Proceed();
        }
    }

    // <target>
    internal class Targets
    {
        private interface I<T>
        {
            [Aspect]
            void M( T x );
        }

        private class C : I<int>
        {
            public void M( int x ) { }

            // This one should not be transformed.
            public void M( string x ) { }
        }
    }
}