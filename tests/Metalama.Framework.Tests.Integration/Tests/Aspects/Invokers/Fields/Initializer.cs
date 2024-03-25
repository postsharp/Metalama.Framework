using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.BaseClass;
using System;
using System.Linq;

[assembly: AspectOrder(typeof(OverrideAndInitializeAttribute), typeof(ResetInitializerAttribute))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.BaseClass;

/*
 * Tests invokers targeting a field declared in the base class.
 */

public class ResetInitializerAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var f = builder.Target.Fields.OfName("TestField").Single();
        var p = builder.Target.Properties.OfName("TestProperty").Single();

        builder.Advice.AddInitializer(builder.Target, nameof(Template), Metalama.Framework.Advising.InitializerKind.BeforeInstanceConstructor, args: new { fieldOrProperty = f });
        builder.Advice.AddInitializer(builder.Target, nameof(Template), Metalama.Framework.Advising.InitializerKind.BeforeInstanceConstructor, args: new { fieldOrProperty = p });
    }

    [Template]
    public void Template([CompileTime] IFieldOrProperty fieldOrProperty)
    {
        fieldOrProperty.With(Metalama.Framework.Code.Invokers.InvokerOptions.Base).Value = default;
    }
}

public class OverrideAndInitializeAttribute : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        var f = builder.Target.Fields.OfName("TestField").Single();
        var p = builder.Target.Properties.OfName("TestProperty").Single();

        builder.Advice.Override(f, nameof(Template));
        builder.Advice.Override(p, nameof(Template));
    }

    [Template]
    public dynamic? Template
    {
        get
        {
            Console.WriteLine("Overridden");
            return meta.Proceed();
        }

        set
        {
            Console.WriteLine("Overridden");
            meta.Proceed();
        }
    }
}

// <target>
[ResetInitializer]
[OverrideAndInitialize]
public class TestClass
{
    public int TestField;

    public int TestProperty { get; set; }
}