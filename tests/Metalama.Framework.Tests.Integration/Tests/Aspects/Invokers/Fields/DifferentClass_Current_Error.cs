﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.DifferentClass_Current_Error;

/*
 * Tests that current invoker targeting a field declared in a different class produces an error.
 */

public class InvokerAspect : FieldOrPropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
    {
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(GetTemplate),
            nameof(SetTemplate),
            new { target = ((INamedType)builder.Target.DeclaringType.Fields.Single().Type).FieldsAndProperties.OfName("Field").Single() });
    }

    [Template]
    public dynamic? GetTemplate([CompileTime] IFieldOrProperty target)
    {
        _ = target.With((IExpression)meta.Target.Property.DeclaringType.Fields.Single().Value!, InvokerOptions.Current).Value;

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate([CompileTime] IFieldOrProperty target)
    {
        target.With((IExpression)meta.Target.Property.DeclaringType.Fields.Single().Value!, InvokerOptions.Current).Value = 42;

        meta.Proceed();
    }
}

public class DifferentClass
{
    public int Field;
}

// <target>
public class TargetClass
{
    private DifferentClass? instance;

    [InvokerAspect]
    public int Invoker
    {
        get { return 0; }
        set { }
    }
}
