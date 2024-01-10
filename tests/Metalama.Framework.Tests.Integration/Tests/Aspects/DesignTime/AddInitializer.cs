#if TEST_OPTIONS
// @DesignTime
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.DesignTime.AddInitializer;

// This tests that adding initializer is not visible at deisgn time.

public class Aspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.AddInitializer(builder.Target, StatementFactory.Parse("x = 42;"), InitializerKind.BeforeInstanceConstructor);
    }
}

// <target>
[Aspect]
internal partial class RegularClass
{
    int x;
}

// <target>
[Aspect]
internal partial class ClassWithPrimaryConstructor()
{
    int x;
}

#endif