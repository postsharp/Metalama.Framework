using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Inheritance.InheritedMethodAttribute
{
    internal class Aspect : OverrideMethodAspect, IInheritedAspect
    {
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