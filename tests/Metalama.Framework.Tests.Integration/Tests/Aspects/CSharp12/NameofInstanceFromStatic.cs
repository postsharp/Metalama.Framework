#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.CSharp12.NameofInstanceFromStatic;

#pragma warning disable CS0649 // Field is never assigned

public class TheAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.Override(builder.Target, nameof(M));
    }

    string? p;

    [Template]
    static string M() => meta.Proceed() + nameof(p.Length);
}

public class C
{
    string? p;

    [TheAspect]
    static string M() => nameof(p.Length);
}

#endif