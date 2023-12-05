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

#pragma warning disable CS0169 // field is never used

public class Aspect : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.AddInitializer(builder.Target, nameof(InitializerTemplate), InitializerKind.BeforeInstanceConstructor);
    }

    [Template]
    void InitializerTemplate()
    {
        foreach (var fieldOrProperty in meta.Target.Type.FieldsAndProperties)
        {
            if (!fieldOrProperty.IsImplicitlyDeclared)
            {
                fieldOrProperty.Value = fieldOrProperty.Name;
            }
        }
    }
}

// <target>
[Aspect]
class TargetCode()
{
    string? f;
    string? f1, f2;

    public string? Property1 { get; }
    public string? Property2 { get; set; }
}

#endif