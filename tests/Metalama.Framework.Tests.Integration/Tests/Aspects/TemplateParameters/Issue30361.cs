using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.TemplateParameters.Issue30361;

internal class NormalizeAttribute : FieldOrPropertyAspect
{
    public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
    {
        builder.Advise.Override( builder.Target, nameof(this.OverrideProperty) );
    }

    [Template]
    private string OverrideProperty
    {
        set => meta.Target.FieldOrProperty.Value = value?.Trim().ToLowerInvariant();
    }
}

// <target>
internal class Foo
{
    [Normalize] public string? Property { get; set; }
}