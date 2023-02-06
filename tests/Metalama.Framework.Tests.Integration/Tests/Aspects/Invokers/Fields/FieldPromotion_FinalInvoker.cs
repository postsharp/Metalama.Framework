using System;
using System.Linq;
using Castle.DynamicProxy.Generators;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_FinalInvoker;

[assembly: AspectOrder( typeof(After), typeof(Override), typeof(Before) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.FieldPromotion_FinalInvoker;

public class Override : TypeAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder.Advice.OverrideAccessors( builder.Target.Fields.OfName( "Field" ).Single(), nameof(Template), nameof(Template) );
        builder.Advice.OverrideAccessors( builder.Target.Fields.OfName( "Field_Static" ).Single(), nameof(Template), nameof(Template) );
    }

    [Template]
    public dynamic? Template()
    {
        Console.WriteLine( "Override" );

        return meta.Proceed();
    }

    [Introduce]
    public void Introduced()
    {
        _ = meta.Target.Type.Fields.OfName( "Field" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_Static" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field_Static" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().With( InvokerOptions.Final ).Value = 42;
    }
}

public class Before : TypeAspect
{
    [Introduce]
    public void IntroducedBefore()
    {
        _ = meta.Target.Type.Fields.OfName( "Field" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_Static" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field_Static" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().With( InvokerOptions.Final ).Value = 42;
    }
}

public class After : TypeAspect
{
    [Introduce]
    public void IntroducedAfter()
    {
        _ = meta.Target.Type.Properties.OfName( "Field" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Properties.OfName( "Field" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Properties.OfName( "Field_Static" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Properties.OfName( "Field_Static" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().With( InvokerOptions.Final ).Value = 42;
        _ = meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().With( InvokerOptions.Final ).Value;
        meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().With( InvokerOptions.Final ).Value = 42;
    }
}

// <target>
[Before]
[Override]
[After]
public class Target
{
    public int Field;
    public static int Field_Static;
    public int Field_NoOverride;
    public static int Field_Static_NoOverride;
}