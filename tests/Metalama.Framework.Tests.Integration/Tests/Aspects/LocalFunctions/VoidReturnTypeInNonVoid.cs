using System.Threading;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.VoidReturnTypeInNonVoid;

public class Override : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        void LocalFunction()
        {
            // Invalid code generated here.
            meta.Proceed();
        }

        LocalFunction();
        return default;

    }
}

// <target>
internal class TargetClass
{
    [Override]
    private int Method() => 42;

    [Override]
    private int Method_ExpressionBody()
    {
        return 42;
    }
}