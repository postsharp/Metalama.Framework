using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_MemberAndType;

[assembly: AspectOrder( typeof(Aspect2), typeof(Aspect1) )]

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_MemberAndType
{
    public class AspectBase : TypeAspect
    {
        [Template]
        public void Template()
        {
            Console.WriteLine($"{((IMemberOrNamedType)meta.Target.Declaration).Name}: {meta.AspectInstance.AspectClass.ShortName}");
        }
    }

    public class Aspect1 : AspectBase
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.Initialize(builder.Target, nameof(Template), InitializationReason.Constructing);
        }
    }

    public class Aspect2 : AspectBase
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.Initialize(builder.Target.Properties.First(), nameof(Template), InitializationReason.Constructing);
        }
    }

    // <target>
    [Aspect1]
    [Aspect2]
    public class TargetCode
    {
        public TargetCode()
        {
        }

        public int Foo { get; }
    }
}