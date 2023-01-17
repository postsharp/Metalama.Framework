using System.Threading;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.GenericReturnType;

public class Override : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Advise.Override( builder.Target, nameof(Template), args: new { T = builder.Target.ReturnType } );
    }

    [Template]
    private T Template<[CompileTime] T>()
    {
        T LocalFunction()
        {
            return meta.Proceed();
        }

        return LocalFunction();
    }
}

// <target>
internal class TargetClass
{
    [Override]
    private int Method() => 5;
}