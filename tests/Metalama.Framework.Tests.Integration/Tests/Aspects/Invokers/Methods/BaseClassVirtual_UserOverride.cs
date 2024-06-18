﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual_UserOverride;

/*
 * Tests invokers targeting a virtual method declared in the base class which is overridden by a C# method.
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

public class BaseClass
{
    public virtual void Method() { }
}

// <target>
public class TargetClass : BaseClass
{
    public override void Method() { }

    [InvokerAspect]
    public void Invoker() { }
}