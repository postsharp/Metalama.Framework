using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CallingTemplates.RunTimeParameter;

class Aspect : TypeAspect
{
    [Introduce]
    int Add(int a)
    {
        AddImpl(a, 1, 1, meta.CompileTime(1));

        // TODO: throw meta.Unreachable();?
        throw new Exception();
    }

    [Template]
    void AddImpl(int a, int b, [CompileTime] int c, [CompileTime] int d)
    {
        meta.Return(a + b + c + d);
    }
}

// <target>
[Aspect]
class TargetCode
{
}