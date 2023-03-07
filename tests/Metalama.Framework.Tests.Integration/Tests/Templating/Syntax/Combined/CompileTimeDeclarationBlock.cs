using System;
using System.Reflection.Emit;
using Metalama.Framework.Aspects;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Templating.Syntax.Combined.CompileTimeDeclarationBlock;

[CompileTime]
class Aspect
{
    [TestTemplate]
    dynamic? Template()
    {
        // This tests that statements that cannot be embedded are not embedded in template code.

        foreach (var member in meta.Target.Type.Methods)
        {
            var id = member.Name;
        }

        if (meta.Target.Method is { } method)
        {
            var id = method.Name;
        }

        return null!;
    }
}

class TargetCode
{
    void Method()
    {
    }
}