using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Inheritance.InheritedMethodInGenericTypettribute
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
        private class BaseClass<T>
        {
            [Aspect]
            public virtual T M( T a ) => a;
        }

        private class DerivedClass : BaseClass<int>
        {
            public override int M( int a ) => a + 1;
        }
    }
}