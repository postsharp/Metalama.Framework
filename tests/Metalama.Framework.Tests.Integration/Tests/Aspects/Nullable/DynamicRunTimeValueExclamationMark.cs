#if TEST_OPTIONS
// @ClearIgnoredDiagnostics to verify nullability warnings
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Nullable.DynamicRunTimeValueExclamationMark;

internal class Aspect : OverrideMethodAspect
{
    public override dynamic OverrideMethod()
    {
        var nn = meta.CompileTime("foo");

        Console.WriteLine(meta.RunTime(nn)!.ToString());

        var n = meta.CompileTime((string?)"bar");

        Console.WriteLine(meta.RunTime(n)!.ToString());

        return null!;
    }
}

class TargetCode
{
    // <target>
    [Aspect]
    void M() { }
}