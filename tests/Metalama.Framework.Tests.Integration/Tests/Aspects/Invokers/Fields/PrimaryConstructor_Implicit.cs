using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using System.Diagnostics;
using System.Linq;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PrimaryConstructor_Implicit;

/*
 * Tests invokers targeting a field declared implicitly by referencing a primary constructor parameter.
 */

public class InvokerAspect : FieldOrPropertyAspect
{
    public override void BuildAspect(IAspectBuilder<IFieldOrProperty> builder)
    {
        Debugger.Break();
        builder.Advice.OverrideAccessors(
            builder.Target,
            nameof(GetTemplate),
            nameof(SetTemplate),
            new { target = builder.Target.DeclaringType!.FieldsAndProperties.OfName("field").Single() });
    }

    [Template]
    public dynamic? GetTemplate([CompileTime] IFieldOrProperty target)
    {
        meta.InsertComment("Invoke this.field");
        _ = target.Value;
        meta.InsertComment("Invoke this.field");
        _ = target.With(InvokerOptions.Base).Value;
        meta.InsertComment("Invoke this.field");
        _ = target.With(InvokerOptions.Current).Value;
        meta.InsertComment("Invoke this.field");
        _ = target.With(InvokerOptions.Final).Value;

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate([CompileTime] IFieldOrProperty target)
    {
        meta.InsertComment("Invoke this.field");
        target.Value = 42;
        meta.InsertComment("Invoke this.field");
        target.With(InvokerOptions.Base).Value = 42;
        meta.InsertComment("Invoke this.field");
        target.With(InvokerOptions.Current).Value = 42;
        meta.InsertComment("Invoke this.field");
        target.With(InvokerOptions.Final).Value = 42;

        meta.Proceed();
    }
}

// <target>
public class TargetClass(int field)
{
    [InvokerAspect]
    public int Invoker
    {
        get { return 0; }
        set {}
    }

    public int Foo()
    {
        return field;
    }
}
