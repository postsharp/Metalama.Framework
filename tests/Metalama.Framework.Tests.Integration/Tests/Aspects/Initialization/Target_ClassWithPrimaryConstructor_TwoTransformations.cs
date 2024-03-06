#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Initialization.Target_ClassWithPrimaryConstructor_TwoTransformations;

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddInitializer(builder.Target, StatementFactory.Parse($"Property = 13;"), InitializerKind.BeforeInstanceConstructor);
        builder.Advice.AddInitializer(builder.Target, StatementFactory.Parse($"Property = 42;"), InitializerKind.BeforeInstanceConstructor);
    }
}

#pragma warning disable CS0414

// <target>
[Aspect]
abstract class TargetCode()
{
    public int Property { get; }
}

#endif