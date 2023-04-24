using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.BaseClassStatic_UserHidden;

/*
 * Tests invokers targeting a method declared in the base class, which is hidden by a C# method.
 */

public class InvokerAspect : MethodAspect
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
        meta.InsertComment("Invoke BaseClass.Method");
        target.Invoke();
        meta.InsertComment("Invoke BaseClass.Method");
        target.With(InvokerOptions.Base).Invoke();
        meta.InsertComment("Invoke BaseClass.Method");
        target.With(InvokerOptions.Current).Invoke();
        meta.InsertComment("Invoke BaseClass.Method");
        target.With(InvokerOptions.Final).Invoke();

        return meta.Proceed();
    }
}

public class BaseClass
{
    public static void Method()
    {
    }
}

// <target>
public class TargetClass : BaseClass
{
    public new static void Method()
    {
    }

    [InvokerAspect]
    public void Invoker()
    {
    }
}