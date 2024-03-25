using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Properties.VoidTemplate;

public class OverrideAttribute : PropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        base.BuildAspect(builder);

        builder.Advice.OverrideAccessors(builder.Target, nameof(OverrideMethod));
    }

    [Template]
    public void OverrideMethod()
    {
        var value = meta.Proceed();
        meta.Return(value == null ? default : value);
    }
}

// <target>
internal class TargetClass
{
    [Override]
    int P { get; set; }
}
