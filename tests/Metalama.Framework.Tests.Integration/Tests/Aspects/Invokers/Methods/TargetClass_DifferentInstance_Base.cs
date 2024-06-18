﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.TargetClass_DifferentInstance_Base;

/*
 * Tests base invoker targeting a method from a different instance.
 */

public class InvokerAspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        var anotherMethod = builder.Target.DeclaringType!.Methods.OfName( "Method" ).Single();

        builder.Advice.Override(
            anotherMethod,
            nameof(AnotherMethodTemplate) );

        builder.Override(
            nameof(Template),
            new { target = anotherMethod } );
    }

    [Template]
    public dynamic? Template( [CompileTime] IMethod target )
    {
        target.With( (IExpression)meta.Target.Method.Parameters[0].Value!, InvokerOptions.Base ).Invoke();

        return meta.Proceed();
    }

    [Template]
    public void AnotherMethodTemplate()
    {
        Console.WriteLine();
    }
}

// <target>
public class TargetClass
{
    public void Method() { }

    [InvokerAspect]
    public void Invoker( TargetClass instance ) { }
}