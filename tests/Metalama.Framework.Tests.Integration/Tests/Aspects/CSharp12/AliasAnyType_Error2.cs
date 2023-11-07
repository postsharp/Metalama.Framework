#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using unsafe IntPointer = int*;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.AliasAnyType_Error2;

public class TheAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(M));
    }

    [CompileTime]
    unsafe void CompileTimeMethod(IntPointer ptr) { }

    [Template]
    unsafe static void M(IntPointer ptr) => meta.Proceed();

    [Introduce]
    unsafe static void Introduced(IntPointer ptr) { }
}

public class C
{
    [TheAspect]
    static unsafe void M(IntPointer ptr)
    {
    }
}

#endif