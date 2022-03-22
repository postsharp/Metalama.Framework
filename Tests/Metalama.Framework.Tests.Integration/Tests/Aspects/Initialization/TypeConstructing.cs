using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing;

[assembly: AspectOrder(typeof(Aspect2), typeof(Aspect1))]

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeConstructing
{
    internal class AspectBase : TypeAspect
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

    internal class Aspect1 : AspectBase
    {
    }

    internal class Aspect2 : AspectBase
    {
    }

    // <target>
    [Aspect1]
    [Aspect2]
    internal class TargetCode
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