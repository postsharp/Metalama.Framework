using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.BaseClassVirtual_DifferentInstance_Base;

/*
 * Tests base invoker targeting a method from a different instance.
 */

public class InvokerAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        var anotherMethodBase = builder.Target.DeclaringType!.BaseType!.Methods.OfName("Method").Single();

        builder.Advice.Override(
            builder.Target,
            nameof(Template),
            new { target = anotherMethodBase });
    }

    [Template]
    public dynamic? Template(IMethod target)
    {
        target.With((IExpression)meta.Target.Method.Parameters[0].Value!, InvokerOptions.Base).Invoke();

        return meta.Proceed();
    }

    [Template]
    public void AnotherMethodTemplate()
    {
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
    [InvokerAspect]
    public void Invoker(TargetClass instance)
    {
    }
}
