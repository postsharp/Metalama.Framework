﻿using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Methods.TargetClass_AspectOverride;
using System.Linq;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(InvokerAfterAspect), typeof(OverrideAspect), typeof(InvokerBeforeAspect) )]

namespace Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Methods.TargetClass_AspectOverride;

/*
 * Tests invokers targeting a method declared in the target class which is then overridden by an aspect.
 */

public class InvokerBeforeAspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Override(
            nameof(Template),
            new { target = builder.Target.DeclaringType!.Methods.OfName( "Method" ).Single() } );
    }

    [Template]
    public dynamic? Template( [CompileTime] IMethod target )
    {
        meta.InsertComment( "Invoke this.Method" );
        target.Invoke();
        meta.InsertComment( "Invoke this.Method_Source" );
        target.With( InvokerOptions.Base ).Invoke();
        meta.InsertComment( "Invoke this.Method_Source" );
        target.With( InvokerOptions.Current ).Invoke();
        meta.InsertComment( "Invoke this.Method" );
        target.With( InvokerOptions.Final ).Invoke();

        return meta.Proceed();
    }
}

public class OverrideAspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Override( nameof(Template) );
    }

    [Template]
    public void Template()
    {
        meta.InsertComment( "Invoke this.Method_Source" );
        meta.Target.Method.Invoke();
        meta.InsertComment( "Invoke this.Method_Source" );
        meta.Target.Method.With( InvokerOptions.Base ).Invoke();
        meta.InsertComment( "Invoke this.Method" );
        meta.Target.Method.With( InvokerOptions.Current ).Invoke();
        meta.InsertComment( "Invoke this.Method" );
        meta.Target.Method.With( InvokerOptions.Final ).Invoke();
        meta.InsertComment( "Invoke this.Method_Source" );
        meta.Proceed();
    }
}

public class InvokerAfterAspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Override(
            nameof(Template),
            new { target = builder.Target.DeclaringType!.Methods.OfName( "Method" ).Single() } );
    }

    [Template]
    public dynamic? Template( [CompileTime] IMethod target )
    {
        meta.InsertComment( "Invoke this.Method" );
        target.Invoke();
        meta.InsertComment( "Invoke this.Method" );
        target.With( InvokerOptions.Base ).Invoke();
        meta.InsertComment( "Invoke this.Method" );
        target.With( InvokerOptions.Current ).Invoke();
        meta.InsertComment( "Invoke this.Method" );
        target.With( InvokerOptions.Final ).Invoke();
        meta.InsertComment( "Invoke this.Method" );

        return meta.Proceed();
    }
}

// <target>
public class TargetClass
{
    [OverrideAspect]
    public void Method() { }

    [InvokerBeforeAspect]
    public void InvokerBefore() { }

    [InvokerAfterAspect]
    public void InvokerAfter() { }
}