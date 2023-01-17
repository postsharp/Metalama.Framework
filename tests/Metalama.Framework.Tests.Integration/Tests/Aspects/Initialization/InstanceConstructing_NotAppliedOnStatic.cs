using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_NotAppliedOnStatic
{
    public class Aspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advise.AddInitializer( builder.Target, nameof(Template), InitializerKind.BeforeInstanceConstructor );
        }

        [Template]
        public void Template()
        {
            Console.WriteLine( $"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}" );
        }
    }

    // <target>
    [Aspect]
    public class TargetCode
    {
        public TargetCode() { }

        static TargetCode() { }

        private int Method( int a )
        {
            return a;
        }
    }
}