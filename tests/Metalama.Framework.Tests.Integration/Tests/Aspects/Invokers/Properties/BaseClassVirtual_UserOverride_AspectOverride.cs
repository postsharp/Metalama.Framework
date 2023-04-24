﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.BaseClassVirtual_UserOverride_AspectOverride;

[assembly: AspectOrder(typeof(InvokerAfterAspect), typeof(OverrideAspect), typeof(InvokerBeforeAspect))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.BaseClassVirtual_UserOverride_AspectOverride;

/*
 * Tests invokers targeting a virtual method declared in the base class which is overridden by a C# method which is then overridden by an aspect.
 */

public class InvokerBeforeAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(
            builder.Target,
            nameof(Template),
            new { target = builder.Target.DeclaringType!.BaseType!.Methods.OfName("Method").Single() });
    }

    [Template]
    public dynamic? Template([CompileTime] IMethod target)
    {
        meta.InsertComment("Invoke this.Method");
        target.Invoke();
        meta.InsertComment("Invoke this.Method_Source");
        target.With(InvokerOptions.Base).Invoke();
        meta.InsertComment("Invoke this.Method_Source");
        target.With(InvokerOptions.Current).Invoke();
        meta.InsertComment("Invoke this.Method");
        target.With(InvokerOptions.Final).Invoke();

        return meta.Proceed();
    }
}

public class OverrideAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(builder.Target, nameof(Template));
    }

    [Template]
    public void Template()
    {
        meta.InsertComment("Invoke this.Method_Source");
        meta.Target.Method.Invoke();
        meta.InsertComment("Invoke this.Method_Source");
        meta.Target.Method.With(InvokerOptions.Base).Invoke();
        meta.InsertComment("Invoke this.Method");
        meta.Target.Method.With(InvokerOptions.Current).Invoke();
        meta.InsertComment("Invoke this.Method");
        meta.Target.Method.With(InvokerOptions.Final).Invoke();
        meta.InsertComment("Invoke this.Method_Source");
        meta.Proceed();
    }
}

public class InvokerAfterAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(
            builder.Target,
            nameof(Template),
            new { target = builder.Target.DeclaringType!.BaseType!.Methods.OfName("Method").Single() });
    }

    [Template]
    public dynamic? Template([CompileTime] IMethod target)
    {
        meta.InsertComment("Invoke this.Method");
        target.Invoke();
        meta.InsertComment("Invoke this.Method");
        target.With(InvokerOptions.Base).Invoke();
        meta.InsertComment("Invoke this.Method");
        target.With(InvokerOptions.Current).Invoke();
        meta.InsertComment("Invoke this.Method");
        target.With(InvokerOptions.Final).Invoke();
        meta.InsertComment("Invoke this.Method");
        return meta.Proceed();
    }
}

public class BaseClass
{
    public virtual void Method()
    {
    }
}

// <target>
public class TargetClass : BaseClass
{
    [OverrideAspect]
    public override void Method()
    {
    }

    [InvokerBeforeAspect]
    public void InvokerBefore()
    {
    }

    [InvokerAfterAspect]
    public void InvokerAfter()
    {
    }
}
