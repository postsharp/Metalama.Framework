using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Constructors.IntroducedConstructor;

public class InvokerAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        var c = builder.Advice.IntroduceConstructor(
            builder.Target.DeclaringType!,
            nameof(ConstructorTemplate),
            buildConstructor: b =>
            {
                b.AddParameter("x", typeof(int));
            });

        builder.Advice.Override(
            builder.Target,
            nameof(Template),
            new { target = c.Declaration });
    }

    [Template]
    public void ConstructorTemplate()
    {
    }

    [Template]
    public dynamic? Template([CompileTime] IConstructor target)
    {
        meta.InsertComment("Invoke new <target>(42);");
        target.Invoke(42);

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