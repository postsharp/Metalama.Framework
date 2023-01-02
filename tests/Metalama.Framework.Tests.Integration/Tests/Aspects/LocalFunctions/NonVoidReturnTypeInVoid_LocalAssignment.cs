using System;
using System.Threading;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameter.NonVoidReturnTypeInVoid_LocalAssignment;

public class Override : OverrideMethodAspect
{
    public override dynamic? OverrideMethod()
    {
        int LocalFunction()
        {
            var x = meta.Proceed();
            return x?.GetHashCode() ?? 0;
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