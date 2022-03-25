using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing;

[assembly: AspectOrder(typeof(Aspect2), typeof(Aspect1))]

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing
{
    public class AspectBase : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.Initialize(builder.Target, nameof(Template), InitializationReason.TypeConstructing);
        }

        [Template]
        public void Template()
        {
            Console.WriteLine($"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}");
        }
    }

    public class Aspect1 : AspectBase
    {
    }

    public class Aspect2 : AspectBase
    {
    }

    // <target>
    [Aspect1]
    [Aspect2]
    public class TargetCode
    {
        static TargetCode()
        {
        }

        private int Method(int a)
        {
            return a;
        }
    }
}