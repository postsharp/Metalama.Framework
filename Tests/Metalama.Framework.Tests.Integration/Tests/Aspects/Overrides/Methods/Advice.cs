using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Overrides.Methods.Advice
{
    public class OverrideAttribute : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.OverrideMethod( builder.Target.Methods.OfName( "TargetMethod" ).Single(), nameof(Template) );
        }

        [Template]
        public dynamic? Template()
        {
            Console.WriteLine( "This is the overriding method." );

            return meta.Proceed();
        }
    }

    // <target>
    [Override]
    internal class TargetClass
    {
        public void TargetMethod()
        {
            Console.WriteLine( "This is the original method." );
        }
    }
}