using System;
using Metalama.Framework.Aspects;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.NonVoidReturnTypeInVoid;

public class Override : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        int LocalFunction()
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
    private void Method()
    {
        Console.WriteLine();
    }

    [Override]
    private void Method_ExpressionBody() => Console.WriteLine();
}