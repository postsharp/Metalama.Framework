﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.BaseClassVirtual_UserHidden_AspectOverride;

[assembly: AspectOrder(typeof(InvokerAfterAspect), typeof(OverrideAspect), typeof(InvokerBeforeAspect))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.BaseClassVirtual_UserHidden_AspectOverride;

/*
 * Tests invokers targeting a virtual property declared in the base class which is hidden by a C# property which is then overridden by an aspect.
 */

public class InvokerBeforeAspect : PropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(GetTemplate),
            nameof(SetTemplate),
            new { target = builder.Target.DeclaringType!.BaseType!.Properties.OfName("Property").Single() });
    }

    [Template]
    public dynamic? GetTemplate([CompileTime] IProperty target)
    {
        meta.InsertComment("Invoke this.Property");
        _ = target.Value;
        meta.InsertComment("Invoke this.Property_Source");
        _ = target.With(InvokerOptions.Base).Value;
        meta.InsertComment("Invoke this.Property_Source");
        _ = target.With(InvokerOptions.Current).Value;
        meta.InsertComment("Invoke this.Property");
        _ = target.With(InvokerOptions.Final).Value;

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate([CompileTime] IProperty target)
    {
        meta.InsertComment("Invoke this.Property");
        target.Value = 42;
        meta.InsertComment("Invoke this.Property_Source");
        target.With(InvokerOptions.Base).Value = 42;
        meta.InsertComment("Invoke this.Property_Source");
        target.With(InvokerOptions.Current).Value = 42;
        meta.InsertComment("Invoke this.Property");
        target.With(InvokerOptions.Final).Value = 42;

        meta.Proceed();
    }
}

public class OverrideAspect : PropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        builder.Advice.OverrideAccessors(builder.Target, nameof(GetTemplate), nameof(SetTemplate));
    }

    [Template]
    public dynamic? GetTemplate()
    {
        meta.InsertComment("Invoke this.Property_Source");
        _ = meta.Target.Property.Value;
        meta.InsertComment("Invoke this.Property_Source");
        _ = meta.Target.Property.With(InvokerOptions.Base).Value;
        meta.InsertComment("Invoke this.Property");
        _ = meta.Target.Property.With(InvokerOptions.Current).Value;
        meta.InsertComment("Invoke this.Property");
        _ = meta.Target.Property.With(InvokerOptions.Final).Value;
        meta.InsertComment("Invoke this.Property_Source");
        return meta.Proceed();
    }

    [Template]
    public void SetTemplate()
    {
        meta.InsertComment("Invoke this.Property_Source");
        meta.Target.Property.Value = 42;
        meta.InsertComment("Invoke this.Property_Source");
        meta.Target.Property.With(InvokerOptions.Base).Value = 42;
        meta.InsertComment("Invoke this.Property");
        meta.Target.Property.With(InvokerOptions.Current).Value = 42;
        meta.InsertComment("Invoke this.Property");
        meta.Target.Property.With(InvokerOptions.Final).Value = 42;
        meta.InsertComment("Invoke this.Property_Source");
        meta.Proceed();
    }
}

public class InvokerAfterAspect : PropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(GetTemplate),
            nameof(SetTemplate),
            new { target = builder.Target.DeclaringType!.BaseType!.Properties.OfName("Property").Single() });
    }

    [Template]
    public dynamic? GetTemplate([CompileTime] IProperty target)
    {
        meta.InsertComment("Invoke this.Property");
        _ = target.Value;
        meta.InsertComment("Invoke this.Property");
        _ = target.With(InvokerOptions.Base).Value;
        meta.InsertComment("Invoke this.Property");
        _ = target.With(InvokerOptions.Current).Value;
        meta.InsertComment("Invoke this.Property");
        _ = target.With(InvokerOptions.Final).Value;
        meta.InsertComment("Invoke this.Property");
        return meta.Proceed();
    }

    [Template]
    public void SetTemplate([CompileTime] IProperty target)
    {
        meta.InsertComment("Invoke this.Property");
        target.Value = 42;
        meta.InsertComment("Invoke this.Property");
        target.With(InvokerOptions.Base).Value = 42;
        meta.InsertComment("Invoke this.Property");
        target.With(InvokerOptions.Current).Value = 42;
        meta.InsertComment("Invoke this.Property");
        target.With(InvokerOptions.Final).Value = 42;
        meta.InsertComment("Invoke this.Property");
        meta.Proceed();
    }
}

public class BaseClass
{
    public virtual int Property
    {
        get { return 0; }
        set {}       
    }
}

// <target>
public class TargetClass : BaseClass
{
    [OverrideAspect]
    public new int Property
    {
        get { return 0; }
        set {}       
    }

    [InvokerBeforeAspect]
    public int InvokerBefore
    {
        get { return 0; }
        set {}       
    }

    [InvokerAfterAspect]
    public int InvokerAfter
    {
        get { return 0; }
        set {}       
    }
}
