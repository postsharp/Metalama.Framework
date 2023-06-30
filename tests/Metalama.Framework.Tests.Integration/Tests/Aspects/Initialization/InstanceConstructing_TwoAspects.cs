using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_TwoAspects;

[assembly: AspectOrder(typeof(SecondAspect), typeof(FirstAspect))]

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_TwoAspects
{
    public class FirstAspect : TypeAspect
    {
        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Advice.AddInitializer( builder.Target, nameof(Template), InitializerKind.BeforeInstanceConstructor );
        }

        [Template]
        public void Template()
        {
            Console.WriteLine( $"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName} First" );
        }
    }
    public class SecondAspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.AddInitializer(builder.Target, nameof(Template), InitializerKind.BeforeInstanceConstructor);
        }

        [Template]
        public void Template()
        {
            Console.WriteLine($"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName} Second");
        }
    }

    // <target>
    [FirstAspect]
    [SecondAspect]
    public class TargetCode
    {
        public TargetCode() { }

        public TargetCode( int x ) { }

        private int Method( int a )
        {
            return a;
        }
    }
}