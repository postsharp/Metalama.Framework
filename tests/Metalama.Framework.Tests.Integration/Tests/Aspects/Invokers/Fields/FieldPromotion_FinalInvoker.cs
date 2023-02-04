﻿using System;
using System.Linq;
using Castle.DynamicProxy.Generators;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
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
        _ = meta.Target.Type.Fields.OfName( "Field" ).Single().GetValue( meta.This );
        meta.Target.Type.Fields.OfName( "Field" ).Single().SetValue( meta.This, 42 );
        _ = meta.Target.Type.Fields.OfName( "Field_Static" ).Single().GetValue( null );
        meta.Target.Type.Fields.OfName( "Field_Static" ).Single().SetValue( null, 42 );
        _ = meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().GetValue( meta.This );
        meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().SetValue( meta.This, 42 );
        _ = meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().GetValue( null );
        meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().SetValue( null, 42 );
    }
}

public class Before : TypeAspect
{
    [Introduce]
    public void IntroducedBefore()
    {
        _ = meta.Target.Type.Fields.OfName( "Field" ).Single().GetValue( meta.This );
        meta.Target.Type.Fields.OfName( "Field" ).Single().SetValue( meta.This, 42 );
        _ = meta.Target.Type.Fields.OfName( "Field_Static" ).Single().GetValue( null );
        meta.Target.Type.Fields.OfName( "Field_Static" ).Single().SetValue( null, 42 );
        _ = meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().GetValue( meta.This );
        meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().SetValue( meta.This, 42 );
        _ = meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().GetValue( null );
        meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().SetValue( null, 42 );
    }
}

public class After : TypeAspect
{
    [Introduce]
    public void IntroducedAfter()
    {
        _ = meta.Target.Type.Properties.OfName( "Field" ).Single().GetValue( meta.This );
        meta.Target.Type.Properties.OfName( "Field" ).Single().SetValue( meta.This, 42 );
        _ = meta.Target.Type.Properties.OfName( "Field_Static" ).Single().GetValue( null );
        meta.Target.Type.Properties.OfName( "Field_Static" ).Single().SetValue( null, 42 );
        _ = meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().GetValue( meta.This );
        meta.Target.Type.Fields.OfName( "Field_NoOverride" ).Single().SetValue( meta.This, 42 );
        _ = meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().GetValue( null );
        meta.Target.Type.Fields.OfName( "Field_Static_NoOverride" ).Single().SetValue( null, 42 );
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