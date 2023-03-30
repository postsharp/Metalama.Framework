#if TEST_OPTIONS
// @ClearIgnoredDiagnostics to verify nullability warnings
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.CompileTimeValue;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        var nn = TypedConstant.Create("foo");

        Console.WriteLine(nn.Value?.ToString());
        Console.WriteLine(nn.Value!.ToString());

        return null!;
    }
}

class TargetCode
{
    // <target>
    [Aspect]
    void M() { }
}