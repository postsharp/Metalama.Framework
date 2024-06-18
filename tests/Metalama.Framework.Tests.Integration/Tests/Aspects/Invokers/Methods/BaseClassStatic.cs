﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassStatic;

/*
 * Tests invokers targeting a static method declared in the base class.
 */

public class InvokerAspect : MethodAspect
{
    public override void BuildAspect( IAspectBuilder<IMethod> builder )
    {
        builder.Override(
            nameof(Template),
            new { target = builder.Target.DeclaringType!.BaseType!.Methods.OfName( "Method" ).Single() } );
    }

    [Template]
    public dynamic? Template( [CompileTime] IMethod target )
    {
        meta.InsertComment( "Invoke BaseClass.Method" );
        target.Invoke();
        meta.InsertComment( "Invoke BaseClass.Method" );
        target.With( InvokerOptions.Base ).Invoke();
        meta.InsertComment( "Invoke BaseClass.Method" );
        target.With( InvokerOptions.Current ).Invoke();
        meta.InsertComment( "Invoke BaseClass.Method" );
        target.With( InvokerOptions.Final ).Invoke();

        return meta.Proceed();
    }
}

public class BaseClass
{
    public static void Method() { }
}

// <target>
public class TargetClass : BaseClass
{
    [InvokerAspect]
    public void Invoker() { }
}