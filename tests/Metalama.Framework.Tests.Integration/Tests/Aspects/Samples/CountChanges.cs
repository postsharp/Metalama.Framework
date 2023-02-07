using System;
using System.Collections;
using System.Collections.Generic;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Samples;

public class CountChangesAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var counterProperties = new List<IProperty>();

        foreach (var property in builder.Target.Properties)
        {
            var counterProperty = builder.Advice.IntroduceProperty(
                builder.Target,
                nameof(CounterProperty),
                buildProperty: b => b.Name = $"{property.Name}ChangeCount").Declaration;

            builder.Advice.Override(property, nameof(IncrementCounter), tags: new { CounterProperty = counterProperty });

            counterProperties.Add(counterProperty);
        }

        builder.Advice.IntroduceProperty(
            builder.Target,
            nameof(TotalChanges),
            tags: new { CounterProperties = counterProperties });
    }

    [Template]
    public dynamic? IncrementCounter
    {
        set
        {
            var property = (IProperty)meta.Tags["CounterProperty"]!;

            // This is by default correctly "Base".
            var oldValue = meta.Target.Property.Value;

            meta.Proceed();

            if (oldValue != meta.Target.Property.Value)
            {
                // This should be by default "Current".
                property.With(InvokerOptions.Current).Value = property.With(InvokerOptions.Current).Value + 1;
            }
        }
    }

    [Template]
    public int CounterProperty { get; set; }

    [Template]
    public int TotalChanges
    {
        get
        {
            var properties = (IReadOnlyList<IProperty>)meta.Tags["CounterProperties"]!;
            int sum = 0;

            foreach(var property in properties)
            {
                // This should be by default "Current".
                sum += property.With(InvokerOptions.Current).Value;
            }

            return sum;
        }
    }
}

// <target>
[CountChanges]
class C
{
    public string? Address { get; set; }

    public int Items { get; set; }
}