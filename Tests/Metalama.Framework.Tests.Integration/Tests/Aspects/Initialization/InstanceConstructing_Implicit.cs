using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_Implicit
{
    internal class Aspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.Initialize(builder.Target, nameof(Template), InitializationReason.Constructing);
        }

        [Template]
        public void Template()
        {
            Console.WriteLine($"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}");
        }
    }

    // <target>
    [Aspect]
    internal class TargetCode
    {
        private int Method( int a )
        {
            return a;
        }
    }
}