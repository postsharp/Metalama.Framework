
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Engine.Advising;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33239;

[Inheritable]
public sealed class TestAspect : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        foreach (var property in builder.Target.Properties)
        {
            builder.Advice.OverrideAccessors(property, null, nameof(this.OverridePropertySetter));
        }
    }

    [Template]
    private void OverridePropertySetter(dynamic value)
    {
        if (value != meta.Target.Property.Value)
        {
            meta.Proceed();
        }
    }
}

public interface IBase
{
    int X { get; set; }
}

// <target>
[TestAspect]
public partial class Imp : IBase
{
    int IBase.X { get; set; }
}

