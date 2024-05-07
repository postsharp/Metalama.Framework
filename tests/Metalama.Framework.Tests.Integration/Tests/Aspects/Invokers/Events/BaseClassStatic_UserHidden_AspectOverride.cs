﻿using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride;
using System.Linq;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(InvokerAfterAspect), typeof(OverrideAspect), typeof(InvokerBeforeAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride;

/*
 * Tests invokers targeting a virtual event declared in the base class, which is hidden by a C# event which is then overridden by an aspect.
 */

public class InvokerBeforeAspect : EventAspect
{
    public override void BuildAspect( IAspectBuilder<IEvent> builder )
    {
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(AddTemplate),
            nameof(RemoveTemplate),
            null,
            new { target = builder.Target.DeclaringType!.BaseType!.Events.OfName( "Event" ).Single() } );
    }

    [Template]
    public void AddTemplate( [CompileTime] IEvent target )
    {
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        target.With( InvokerOptions.Base ).Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        target.With( InvokerOptions.Current ).Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.With( InvokerOptions.Final ).Add( meta.RunTime( TargetClass.StaticTarget ) );

        meta.Proceed();
    }

    [Template]
    public void RemoveTemplate( [CompileTime] IEvent target )
    {
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        target.With( InvokerOptions.Base ).Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        target.With( InvokerOptions.Current ).Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.With( InvokerOptions.Final ).Remove( meta.RunTime( TargetClass.StaticTarget ) );

        meta.Proceed();
    }
}

public class OverrideAspect : EventAspect
{
    public override void BuildAspect( IAspectBuilder<IEvent> builder )
    {
        builder.Advice.OverrideAccessors( builder.Target, nameof(AddTemplate), nameof(RemoveTemplate) );
    }

    [Template]
    public void AddTemplate()
    {
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        meta.Target.Event.Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        meta.Target.Event.With( InvokerOptions.Base ).Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        meta.Target.Event.With( InvokerOptions.Current ).Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        meta.Target.Event.With( InvokerOptions.Final ).Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        meta.Proceed();
    }

    [Template]
    public void RemoveTemplate()
    {
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        meta.Target.Event.Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        meta.Target.Event.With( InvokerOptions.Base ).Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        meta.Target.Event.With( InvokerOptions.Current ).Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        meta.Target.Event.With( InvokerOptions.Final ).Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event_Source" );
        meta.Proceed();
    }
}

public class InvokerAfterAspect : EventAspect
{
    public override void BuildAspect( IAspectBuilder<IEvent> builder )
    {
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(AddTemplate),
            nameof(RemoveTemplate),
            null,
            new { target = builder.Target.DeclaringType!.BaseType!.Events.OfName( "Event" ).Single() } );
    }

    [Template]
    public void AddTemplate( [CompileTime] IEvent target )
    {
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.With( InvokerOptions.Base ).Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.With( InvokerOptions.Current ).Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.With( InvokerOptions.Final ).Add( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        meta.Proceed();
    }

    [Template]
    public void RemoveTemplate( [CompileTime] IEvent target )
    {
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.With( InvokerOptions.Base ).Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.With( InvokerOptions.Current ).Remove( meta.RunTime( TargetClass.StaticTarget ) );
        meta.InsertComment( "Invoke TargetClass.Event" );
        target.With( InvokerOptions.Final ).Remove( meta.RunTime( TargetClass.StaticTarget ) );

        meta.Proceed();
    }
}

public class BaseClass
{
    public static event EventHandler Event
    {
        add { }
        remove { }
    }
}

// <target>
public class TargetClass : BaseClass
{
    [OverrideAspect]
    public new static event EventHandler Event
    {
        add { }
        remove { }
    }

    [InvokerBeforeAspect]
    public event EventHandler InvokerBefore
    {
        add { }
        remove { }
    }

    [InvokerAfterAspect]
    public event EventHandler InvokerAfter
    {
        add { }
        remove { }
    }

    public static void StaticTarget( object? sender, EventArgs args ) { }
}