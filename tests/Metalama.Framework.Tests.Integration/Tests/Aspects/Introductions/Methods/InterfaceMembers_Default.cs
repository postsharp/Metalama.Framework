using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.IntegrationTests.Aspects.Introductions.Methods.InterfaceMembers_Default
{
    public class IntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.IntroduceMethod(builder.Target, nameof(Foo), buildMethod: b => b.IsVirtual = true);
        }

        [Template]
        public void Foo()
        {
        }
    }

    // <target>
    [Introduction]
    public interface TargetInterface
    {
    }
}