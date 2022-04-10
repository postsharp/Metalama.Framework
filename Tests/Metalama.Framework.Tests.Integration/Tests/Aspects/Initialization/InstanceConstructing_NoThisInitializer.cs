using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_NoThisConstructor;

namespace Metalama.Framework.Tests.Integration.Aspects.Initialization.InstanceConstructing_NoThisConstructor
{
    public class Aspect : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advices.AddInitializerBeforeInstanceConstructor(builder.Target, nameof(Template));
        }

        [Template]
        public void Template()
        {
            Console.WriteLine($"{meta.Target.Type.Name}: {meta.AspectInstance.AspectClass.ShortName}");
        }
    }

    public class BaseClass
    {
        public BaseClass()
        {
        }

        public BaseClass(int x)
        {
        }
    }

    // <target>
    [Aspect]
    public class TargetCode : BaseClass
    {
        public TargetCode()
        {
        }

        public TargetCode(int x) : base(x)
        {
        }

        public TargetCode(double x) : this()
        {
        }

        private int Method( int a )
        {
            return a;
        }
    }
}