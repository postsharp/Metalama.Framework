﻿#if TEST_OPTIONS
// @RequiredConstant(NET) .Net Framework has different AggregateException.ToString().
#endif

using System;
using System.Collections.Generic;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;

#pragma warning disable CS0067

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Introductions.Interfaces.ExplicitMembers_Programmatic_NoMatch;

public interface IInterface
{
    int InterfaceMethod();

    event EventHandler<EventArgs> InterfaceEvent;

    int InterfaceProperty { get; set; }
}

public class IntroductionAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var explicitImplementation = builder.Advice.ImplementInterface(builder.Target, typeof(IInterface)).ExplicitImplementation;

        Action[] introductions = [
            () => explicitImplementation.IntroduceMethod(nameof(Method)),
            () => explicitImplementation.IntroduceEvent(nameof(Event)),
            () => explicitImplementation.IntroduceProperty(nameof(Property)),
        ];

        var exceptions = new List<Exception>();

        foreach (var introduction in introductions)
        {
            try
            {
                introduction();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                continue;
            }
        }

        throw new AggregateException(exceptions);
    }

    [Template]
    public int Method()
    {
        Console.WriteLine("This is introduced interface member.");

        return meta.Proceed();
    }

    [Template]
    public event EventHandler? Event;

    [Template]
    public int Property { get; set; }
}

// <target>
[Introduction]
public class TargetClass { }
