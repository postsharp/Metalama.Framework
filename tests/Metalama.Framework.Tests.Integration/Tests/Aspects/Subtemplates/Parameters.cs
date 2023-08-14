using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Subtemplates.Parameters;

class Aspect : TypeAspect
{
    [Introduce]
    int Add(int a)
    {
        AddImpl(a, 1, meta.CompileTime(1), 1, meta.CompileTime(1));

        throw new Exception();
    }

    [Template]
    void AddImpl(int a, int b, int c, [CompileTime] int d, [CompileTime] int e)
    {
        meta.Return(a + b + c + d + e);
    }
}

// <target>
[Aspect]
class TargetCode
{
}