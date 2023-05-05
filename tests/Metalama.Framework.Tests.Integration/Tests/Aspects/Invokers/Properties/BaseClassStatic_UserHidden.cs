﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.BaseClassStatic_UserHidden;

/*
 * Tests invokers targeting a property declared in the base class, which is hidden by a C# property.
 */

public class InvokerAspect : PropertyAspect
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
        meta.InsertComment("Invoke BaseClass.Property");
        _ = target.Value;
        meta.InsertComment("Invoke BaseClass.Property");
        _ = target.With(InvokerOptions.Base).Value;
        meta.InsertComment("Invoke BaseClass.Property");
        _ = target.With(InvokerOptions.Current).Value;
        meta.InsertComment("Invoke BaseClass.Property");
        _ = target.With(InvokerOptions.Final).Value;

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate([CompileTime] IProperty target)
    {
        meta.InsertComment("Invoke BaseClass.Property");
        target.Value = 42;
        meta.InsertComment("Invoke BaseClass.Property");
        target.With(InvokerOptions.Base).Value = 42;
        meta.InsertComment("Invoke BaseClass.Property");
        target.With(InvokerOptions.Current).Value = 42;
        meta.InsertComment("Invoke BaseClass.Property");
        target.With(InvokerOptions.Final).Value = 42;

        meta.Proceed();
    }
}

public class BaseClass
{
    public static int Property
    {
        get { return 0; }
        set {}       
    }
}

// <target>
public class TargetClass : BaseClass
{
    public new static int Property
    {
        get { return 0; }
        set {}       
    }

    [InvokerAspect]
    public int Invoker
    {
        get { return 0; }
        set {}
    }
}