﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance;

/*
 * Tests default and final invokers targeting a property declared in a different class.
 */

public class InvokerAspect : PropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(GetTemplate),
            nameof(SetTemplate),
            new { target = ((INamedType)builder.Target.DeclaringType.Fields.Single().Type).Properties.OfName("Property").Single() });
    }

    [Template]
    public dynamic? GetTemplate([CompileTime] IProperty target)
    {
        meta.InsertComment("Invoke instance.Property");
        _ = target.With((IExpression?)meta.Target.Property.DeclaringType.Fields.Single().Value!).Value;
        meta.InsertComment("Invoke instance?.Property");
        _ = target.With((IExpression?)meta.Target.Property.DeclaringType.Fields.Single().Value!, InvokerOptions.NullConditional).Value;
        meta.InsertComment("Invoke instance.Property");
        _ = target.With((IExpression?)meta.Target.Property.DeclaringType.Fields.Single().Value!, InvokerOptions.Final).Value;
        meta.InsertComment("Invoke instance?.Property");
        _ = target.With((IExpression?)meta.Target.Property.DeclaringType.Fields.Single().Value!, InvokerOptions.Final | InvokerOptions.NullConditional).Value;

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate([CompileTime] IProperty target)
    {
        meta.InsertComment("Invoke instance.Property");
        target.With((IExpression?)meta.Target.Property.DeclaringType.Fields.Single().Value!).Value = 42;
        meta.InsertComment("Invoke instance.Property");
        target.With((IExpression?)meta.Target.Property.DeclaringType.Fields.Single().Value!, InvokerOptions.Final).Value = 42;

        meta.Proceed();
    }
}

// <target>
public class TargetClass
{
    public int Property
    {
        get { return 0; }
        set {}       
    }

    private TargetClass? instance;

    [InvokerAspect]
    public int Invoker
    {
        get { return 0; }
        set { }
    }
}