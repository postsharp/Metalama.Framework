﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden;

/*
 * Tests invokers targeting a event declared in the base class, which is hidden by a C# event.
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
            new { target = builder.Target.DeclaringType!.BaseType!.Events.OfName("Event").Single() });
    }

    [Template]
    public void AddTemplate([CompileTime] IEvent target)
    {
        meta.InsertComment("Invoke BaseClass.Event");
        target.Add(meta.RunTime(TargetClass.StaticTarget));
        meta.InsertComment("Invoke BaseClass.Event");
        target.With(InvokerOptions.Base).Add(meta.RunTime(TargetClass.StaticTarget));
        meta.InsertComment("Invoke BaseClass.Event");
        target.With(InvokerOptions.Current).Add(meta.RunTime(TargetClass.StaticTarget));
        meta.InsertComment("Invoke BaseClass.Event");
        target.With(InvokerOptions.Final).Add(meta.RunTime(TargetClass.StaticTarget));

        meta.Proceed();
    }

    [Template]
    public void RemoveTemplate([CompileTime] IEvent target)
    {
        meta.InsertComment("Invoke BaseClass.Event");
        target.Remove(meta.RunTime(TargetClass.StaticTarget));
        meta.InsertComment("Invoke BaseClass.Event");
        target.With(InvokerOptions.Base).Remove(meta.RunTime(TargetClass.StaticTarget));
        meta.InsertComment("Invoke BaseClass.Event");
        target.With(InvokerOptions.Current).Remove(meta.RunTime(TargetClass.StaticTarget));
        meta.InsertComment("Invoke BaseClass.Event");
        target.With(InvokerOptions.Final).Remove(meta.RunTime(TargetClass.StaticTarget));

        meta.Proceed();
    }
}

public class BaseClass
{
    public static event System.EventHandler Event
    {
        add { }
        remove { }
    }
}

// <target>
public class TargetClass : BaseClass
{
    public new static event System.EventHandler Event
    {
        add { }
        remove { }
    }

    [InvokerAspect]
    public event System.EventHandler Invoker
    {
        add { }
        remove { }
    }

    public static void StaticTarget(object? sender, System.EventArgs args) { }
}