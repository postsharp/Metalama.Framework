using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System.Linq;

#pragma warning disable CS0169

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance_Base_Error;

/*
 * Tests that base invoker targeting a property declared in a different class produces an error.
 */

public class InvokerAspect : PropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IProperty> builder)
    {
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(GetTemplate),
            nameof(SetTemplate),
            new { target = ((INamedType)builder.Target.DeclaringType.Fields.Single().Type).Properties.OfName("Property").Single() });
    }

    [Template]
    public dynamic? GetTemplate([CompileTime] IProperty target)
    {
        _ = target.With((IExpression)meta.Target.Property.DeclaringType.Fields.Single().Value!, InvokerOptions.Base).Value;

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate([CompileTime] IProperty target)
    {
        target.With((IExpression)meta.Target.Property.DeclaringType.Fields.Single().Value!, InvokerOptions.Base).Value = 42;

        meta.Proceed();
    }
}

// <target>
public class TargetClass
{
    public int Property
    {
        get { return 0; }
        set {}       
    }

    private TargetClass? instance;

    [InvokerAspect]
    public int Invoker
    {
        get { return 0; }
        set { }
    }
}
