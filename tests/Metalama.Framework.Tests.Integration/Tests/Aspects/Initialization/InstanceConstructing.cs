using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing;

[assembly: AspectOrder( typeof(Aspect1), typeof(Aspect2) )]

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing
{
    public class AspectBase : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.AddInitializer( builder.Target, nameof(Template), InitializerKind.BeforeInstanceConstructor );
        }

        [Template]
        public void Template()
        {
            Console.WriteLine( $"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}" );
        }
    }

    public class Aspect1 : AspectBase { }

    public class Aspect2 : AspectBase { }

    // <target>
    [Aspect1]
    [Aspect2]
    public class TargetCode
    {
        public TargetCode() { }

        private int Method( int a )
        {
            return a;
        }
    }
}