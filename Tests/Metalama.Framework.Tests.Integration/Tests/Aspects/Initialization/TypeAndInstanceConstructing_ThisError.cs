using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.TypeAndInstanceConstructing_ThisError
{
    public class Aspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.Initialize(builder.Target, nameof(Template), InitializationReason.Constructing | InitializationReason.TypeConstructing);
        }

        [Template]
        public void Template()
        {
            Console.WriteLine($"{meta.Target.Type.Name} {meta.This}: {meta.AspectInstance.AspectClass.ShortName}");
        }
    }

    // <target>
    [Aspect]
    public class TargetCode
    {
        public TargetCode()
        {
        }

        static TargetCode()
        {
        }

        private int Method(int a)
        {
            return a;
        }
    }
}