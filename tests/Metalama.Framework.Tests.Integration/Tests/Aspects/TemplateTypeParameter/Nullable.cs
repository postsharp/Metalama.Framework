using System;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.SyntaxBuilders;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateTypeParameters.Nullable;

#pragma warning disable CS0219, CS8618

public class GenerateMethodsAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        base.BuildAspect(builder);

        foreach (var field in builder.Target.Fields)
        {
            builder.Advise.IntroduceMethod(
                builder.Target,
                nameof(this.Use),
                args: new { field = field, T = field.Type },
                buildMethod: m => m.Name = "Use" + field.Name);
        }
    }

    [Template]
    public void Use<[CompileTime] T>(IField field)
    {
        // not allowed by C#:
        // T? value = null;

        T? value = default;
    }
}

// <target>
[GenerateMethods]
public class Target
{
    public string S;
    public string?[] ANS;
    public string? NS;
    public int I;
    public int? NI;
}