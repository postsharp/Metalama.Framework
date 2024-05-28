﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Constructors.Static_Error;

public class InvokerAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(
            builder.Target,
            nameof(Template),
            new { target = builder.Target.DeclaringType!.StaticConstructor });
    }

    [Template]
    public dynamic? Template([CompileTime] IConstructor target)
    {
        meta.InsertComment("Invoke new <target>();");
        target.Invoke();

        return meta.Proceed();
    }
}

// <target>
public class TargetClass
{
    static TargetClass()
    {
    }

    [InvokerAspect]
    public void Invoker()
    {
    }
}