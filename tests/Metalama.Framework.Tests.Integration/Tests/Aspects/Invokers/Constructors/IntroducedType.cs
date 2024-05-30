using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Constructors.IntroducedType;

public class InvokerAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        var t = builder.Advice.IntroduceClass(
            builder.Target.DeclaringType!,
            "IntroducedType" );

        var c = t.IntroduceConstructor(nameof(ConstructorTemplate));

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
        meta.InsertComment("Invoke new <introduced>();");
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