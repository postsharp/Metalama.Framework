using System;
using System.Linq;
using Castle.DynamicProxy.Generators;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker;

[assembly: AspectOrder( typeof(After), typeof(Test), typeof(Before) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.PromotedField_BaseInvoker;

public class Test : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.OverrideAccessors(builder.Target.Fields.OfName("Field").Single(), nameof(Template), nameof(Template));
        builder.Advice.OverrideAccessors(builder.Target.Fields.OfName("Field_Static").Single(), nameof(Template), nameof(Template));
        builder.Advice.OverrideAccessors(builder.Target.ForCompilation(builder.Advice.MutableCompilation).Fields.OfName("IntroducedField").Single(), nameof(Template), nameof(Template));
        builder.Advice.OverrideAccessors(builder.Target.ForCompilation(builder.Advice.MutableCompilation).Fields.OfName("IntroducedField_Static").Single(), nameof(Template), nameof(Template));
    }

    [Template]
    public dynamic? Template()
    {
        Console.WriteLine( "Override" );

        return meta.Proceed();
    }

    [Introduce]
    public int IntroducedField;

    [Introduce]
    public int IntroducedField_Static;

    [Introduce]
    public void Introduced()
    {
        _ = meta.Target.Type.Fields.OfName( "Field" ).Single().With( InvokerOptions.Base ).Value;
        meta.Target.Type.Fields.OfName( "Field" ).Single().With( InvokerOptions.Base ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_Static" ).Single().With( InvokerOptions.Base ).Value;
        meta.Target.Type.Fields.OfName( "Field_Static" ).Single().With( InvokerOptions.Base ).Value = 42;
    }
}

public class Before : TypeAspect
{
    [Introduce]
    public void IntroducedBefore()
    {
        _ = meta.Target.Type.Fields.OfName( "Field" ).Single().With( InvokerOptions.Base ).Value;
        meta.Target.Type.Fields.OfName( "Field" ).Single().With( InvokerOptions.Base ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_Static" ).Single().With( InvokerOptions.Base ).Value;
        meta.Target.Type.Fields.OfName( "Field_Static" ).Single().With( InvokerOptions.Base ).Value = 42;
    }
}

public class After : TypeAspect
{
    [Introduce]
    public void IntroducedAfter()
    {
        _ = meta.Target.Type.Properties.OfName( "Field" ).Single().With( InvokerOptions.Base ).Value;
        meta.Target.Type.Properties.OfName( "Field" ).Single().With( InvokerOptions.Base ).Value = 42;
        _ = meta.Target.Type.Properties.OfName( "Field_Static" ).Single().With( InvokerOptions.Base ).Value;
        meta.Target.Type.Properties.OfName( "Field_Static" ).Single().With( InvokerOptions.Base ).Value = 42;
        _ = meta.Target.Type.Properties.OfName("IntroducedField").Single().With(InvokerOptions.Base).Value;
        meta.Target.Type.Properties.OfName("IntroducedField").Single().With(InvokerOptions.Base).Value = 42;
        _ = meta.Target.Type.Properties.OfName("IntroducedField_Static").Single().With(InvokerOptions.Base).Value;
        meta.Target.Type.Properties.OfName("IntroducedField_Static").Single().With(InvokerOptions.Base).Value = 42;
    }
}

// <target>
[Before]
[Test]
[After]
public class Target
{
    public int Field;
    public static int Field_Static;
}