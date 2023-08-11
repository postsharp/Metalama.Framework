using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code.SyntaxBuilders;
using Metalama.Framework.Engine.Templating;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CallingTemplates.RunTimeParameter_SideEffects;

class Aspect : TypeAspect
{
    [Introduce]
    int Add(int a)
    {
        AddImpl(1, Add(1), 1);

        throw new Exception();
    }

    [Template]
    void AddImpl(int a, int b, [CompileTime] int c)
    {
        meta.Return(a + b + c);
    }
}

// <target>
[Aspect]
class TargetCode
{
}