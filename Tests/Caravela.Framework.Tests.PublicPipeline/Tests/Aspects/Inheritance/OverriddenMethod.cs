using System;
using Caravela.Framework.Aspects;

namespace Caravela.Framework.Tests.PublicPipeline.Aspects.Inheritance.InheritedMethodAttribute
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
        private class BaseClass
        {
            [Aspect]
            public virtual void M() { }
        }

        private class DerivedClass : BaseClass
        {
            public override void M() { }
        }
    }
}