using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.PublicPipeline.Aspects.Inheritance.InheritedGenericTypeAttribute
{
    internal class Aspect : TypeAspect
    {
        public override void BuildAspectClass( IAspectClassBuilder builder )
        {
            builder.IsInherited = true;
        }

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            foreach (var m in builder.Target.Methods)
            {
                builder.Advices.OverrideMethod( m, nameof(Template) );
            }
        }

        [Template]
        private dynamic? Template()
        {
            Console.WriteLine( "Overridden!" );

            return meta.Proceed();
        }
    }

    // <target>
    internal class Targets
    {
        [Aspect]
        private class BaseClass<T>
        {
            private T M( T a ) => a;
        }

        private class DerivedClass : BaseClass<int>
        {
            private void N() { }
        }
    }
}