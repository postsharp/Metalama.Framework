using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_MultipleDeclarations
{
    public class Aspect : TypeAspect
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
    public partial class TargetCode
    {
        public TargetCode()
        {
        }

        private int Method( int a )
        {
            return a;
        }
    }

    // <target>
    public partial class TargetCode
    {
        public TargetCode(int x)
        {
        }
    }

    // <target>
    public partial class TargetCode
    {
    }
}