using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0169 // The field 'Target.i' is never used

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.AbstractPropertyTemplate;

public abstract class AbstractAspect : FieldOrPropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
    {
        builder.Advice.Override(builder.Target, nameof(OverrideProperty));
    }

    [Template]
    public abstract dynamic? OverrideProperty
    {
        get;
        set;
    }
}

public class DerivedAspect : AbstractAspect
{
    public override dynamic? OverrideProperty
    {
        get => throw new NotImplementedException();
        set => throw new NotImplementedException();
    }
}

// <target>
class Target
{
    [DerivedAspect]
    int i;
}