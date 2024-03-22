using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.EventFields.InterfaceEventField;

[assembly: AspectOrder(typeof(OverrideEventAttribute), typeof(OverrideAttribute), typeof(IntroductionAttribute))]

#pragma warning disable CS0414

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.EventFields.InterfaceEventField;

internal interface Interface
{
    event EventHandler? EventField_Default;

    event EventHandler? EventField;
}

internal class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.ImplementInterface(builder.Target, typeof(Interface));
    }

    [InterfaceMember(IsExplicit = true)]
    public event EventHandler? EventField_Default = default;

    [InterfaceMember(IsExplicit = true)]
    public event EventHandler? EventField = Foo;

    [Introduce]
    public static void Foo(object? sender, EventArgs args)
    {
    }
}

internal class OverrideAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Outbound.SelectMany(x => x.Events).AddAspect<OverrideEventAttribute>();
    }
}

public class OverrideEventAttribute : OverrideEventAspect
{
    public OverrideEventAttribute()
    {
    }

    public override void OverrideAdd(dynamic value)
    {
        Console.WriteLine("Overriden add.");
        meta.Proceed();
    }

    public override void OverrideRemove(dynamic value)
    {
        Console.WriteLine("Overriden remove.");
        meta.Proceed();
    }
}

// <target>
[Introduction]
[Override]
internal partial class TargetClass { }