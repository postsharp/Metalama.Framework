using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using MyTuple = (int, int Name);
using unsafe IntPointer = int*;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.AliasAnyType;

public class TheAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(M));
    }

    [CompileTime]
    void CompileTimeMethod(MyTuple tuple) { }

    [Template]
    static void M(MyTuple tuple) => meta.Proceed();

    [Introduce]
    static void Introduced(MyTuple tuple) { }
}

public class C
{
    [TheAspect]
    static unsafe void M(MyTuple tuple, IntPointer ptr)
    {
    }
}