using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_TwoAdvices_Implicit
{
    public class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advices.AddInitializerBeforeInstanceConstructor( builder.Target, nameof(Template), tags: new { name = "first" } );
            builder.Advices.AddInitializerBeforeInstanceConstructor( builder.Target, nameof(Template), tags: new { name = "second" } );
        }

        [Template]
        public void Template()
        {
            Console.WriteLine( $"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName} {meta.Tags["name"]}" );
        }
    }

    // <target>
    [Aspect]
    public class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}