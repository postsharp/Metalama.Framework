using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing_Implicit_MultipleInitializers
{
    public class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.AddInitializer( builder.Target, nameof(Template), InitializerKind.BeforeTypeConstructor );
            builder.Advice.AddInitializer( builder.Target, nameof(Template2), InitializerKind.BeforeTypeConstructor );
        }

        [Template]
        public void Template()
        {
            Console.WriteLine( $"1) {meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}" );
        }

        [Template]
        public void Template2()
        {
            Console.WriteLine($"2) {meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}");
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