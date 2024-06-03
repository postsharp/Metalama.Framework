using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Constructors.ObjectInitializer;

public class InvokerAspect : MethodAspect
{
    public override void BuildAspect(IAspectBuilder<IMethod> builder)
    {
        builder.Advice.Override(
            builder.Target,
            nameof(Template),
            new { target = builder.Target.DeclaringType!.Constructors.Single() });
    }

    [Template]
    public dynamic? Template([CompileTime] IConstructor target)
    {
        meta.InsertComment("Invoke new <target>() { Field = 42, Property = 42 };");

        var x = target.CreateInvokeExpression().WithObjectInitializer(
            (target.DeclaringType.Fields.Single(f => !f.IsImplicitlyDeclared), TypedConstant.Create(42)),
            (target.DeclaringType.Properties.Single(), TypedConstant.Create(42))).Value;

        return meta.Proceed();
    }
}

// <target>
public class TargetClass
{
    public int Field;

    public int Property { get; init; }

    [InvokerAspect]
    public void Invoker()
    {
    }
}