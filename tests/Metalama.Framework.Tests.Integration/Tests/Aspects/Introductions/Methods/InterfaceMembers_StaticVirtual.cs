using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.InterfaceMembers_StaticVirtual
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceMethod(builder.Target, nameof(Foo), IntroductionScope.Static, buildMethod: b => b.IsVirtual = true);
        }

        [Template]
        public void Foo()
        {
        }
    }

    // <target>
    [Introduction]
    public interface TargetInterface : IComparable<TargetInterface>
    {
        int IComparable<TargetInterface>.CompareTo(TargetInterface? i)
        {
            // default implementation
            return 0;
        }

        private int Bar()
        {
            return 42;
        }
    }
}