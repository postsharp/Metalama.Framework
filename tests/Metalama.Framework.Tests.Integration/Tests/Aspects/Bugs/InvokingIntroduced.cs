using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.InvokingIntroduced;

class IntroduceAndInvokeAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        var introducedMember = builder.Advice.IntroduceMethod(builder.Target, nameof(Introduce));
        builder.Advice.IntroduceMethod(builder.Target, nameof(Invoke), buildMethod: method => method.Name = "Invoke0", args: new { introduced = introducedMember.Declaration });

        var interfaceImplementation = builder.Advice.ImplementInterface(builder.Target, typeof(IFoo));
        builder.Advice.IntroduceMethod(builder.Target, nameof(Invoke), args: new { introduced = interfaceImplementation.InterfaceMembers.Single().TargetMember });
    }

    [InterfaceMember]
    public void Bar() { }

    [Template]
    public void Introduce() { }

    [Template]
    public void Invoke(IMethod introduced)
    {
        introduced.Invoke();
    }
}

internal interface IFoo
{
    void Bar();
}

// <target>
[IntroduceAndInvoke]
class Target
{
}