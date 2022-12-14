using System.Threading;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.DifferentReturnType;

public class Override : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        string LocalFunction()
        {
            // Invalid code generated here.
            meta.Proceed();
            return "something";
        }

        var s = LocalFunction();
        return s.Length;
    }
}

// <target>
internal class C
{
    [Override]
    private int Method() => 42;

    [Override]
    private int Method_ExpressionBody()
    {
        return 42;
    }
}