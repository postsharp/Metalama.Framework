using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Aspects.Bugs.TemplateParameterDiscard;

class Aspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    void Template(int arg)
    {
        _ = arg;
    }
}

// <target>
internal class Program
{
    [Aspect]
    void M(int arg) { }
}