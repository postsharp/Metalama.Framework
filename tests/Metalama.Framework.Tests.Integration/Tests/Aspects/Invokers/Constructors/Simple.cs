using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Constructors.Simple;

public class InvokerAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(
            builder.Target,
            nameof(Template),
            new { target = builder.Target.DeclaringType!.BaseType!.Constructors.Single() });
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
    [InvokerAspect]
    public void Invoker()
    {
    }
}