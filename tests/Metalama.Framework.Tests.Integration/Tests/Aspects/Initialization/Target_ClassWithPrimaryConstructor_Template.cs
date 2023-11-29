#if TEST_OPTIONS
// @RequiredConstant(ROSLYN_4_8_0_OR_GREATER)
#endif

#if ROSLYN_4_8_0_OR_GREATER

using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Initialization.Target_ClassWithPrimaryConstructor_Template;

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddInitializer(builder.Target, nameof(InitializerTemplate), InitializerKind.BeforeInstanceConstructor);
    }

    [Template]
    void InitializerTemplate()
    {
        meta.Target.Type.Fields.OfName("x").Single().Value = 42;
    }
}

// <target>
[Aspect]
class TargetCode()
{
    int x;
}

#endif