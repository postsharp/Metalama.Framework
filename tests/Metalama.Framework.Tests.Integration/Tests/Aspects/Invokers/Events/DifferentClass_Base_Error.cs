﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System.Linq;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.DifferentClass_Base_Error;

/*
 * Tests that base invoker targeting a event declared in a different class produces an error.
 */

public class InvokerAspect : EventAspect
{
    public override void BuildAspect(IAspectBuilder<IEvent> builder)
    {
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(AddTemplate),
            nameof(RemoveTemplate),
            null,
            new { target = ((INamedType)builder.Target.DeclaringType.Fields.Single().Type).Events.OfName("Event").Single() });
    }

    [Template]
    public void AddTemplate([CompileTime] IEvent target)
    {
        target.With((IExpression)meta.Target.Event.DeclaringType.Fields.Single().Value!, InvokerOptions.Base).Add(meta.RunTime(TargetClass.StaticTarget));

        meta.Proceed();
    }

    [Template]
    public void RemoveTemplate([CompileTime] IEvent target)
    {
        target.With((IExpression)meta.Target.Event.DeclaringType.Fields.Single().Value!, InvokerOptions.Base).Remove(meta.RunTime(TargetClass.StaticTarget));

        meta.Proceed();
    }
}

public class DifferentClass
{
    public event System.EventHandler Event
    {
        add { }
        remove { }
    }
}

// <target>
public class TargetClass
{
    private DifferentClass? instance;

    [InvokerAspect]
    public event System.EventHandler Invoker
    {
        add { }
        remove { }
    }

    public static void StaticTarget(object? sender, System.EventArgs args) { }
}
