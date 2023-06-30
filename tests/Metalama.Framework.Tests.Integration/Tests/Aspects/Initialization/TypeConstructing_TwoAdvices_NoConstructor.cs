using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_TwoAdvices_NoConstructor
{
    public class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.AddInitializer( builder.Target, nameof(Template), InitializerKind.BeforeInstanceConstructor, tags: new { name = "first" } );
            builder.Advice.AddInitializer( builder.Target, nameof(Template), InitializerKind.BeforeInstanceConstructor, tags: new { name = "second" } );
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
    }
}