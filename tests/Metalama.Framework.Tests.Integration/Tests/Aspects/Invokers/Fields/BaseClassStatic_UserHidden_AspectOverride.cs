﻿using Metalama.Framework.Aspects;
using Metalama.Framework.Advising;
using Metalama.Framework.Code;
using Metalama.Framework.Code.Invokers;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.BaseClassStatic_UserHidden_AspectOverride;
using System.Linq;

[assembly: AspectOrder( AspectOrderDirection.RunTime, typeof(InvokerAfterAspect), typeof(OverrideAspect), typeof(InvokerBeforeAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Fields.BaseClassStatic_UserHidden_AspectOverride;

/*
 * Tests invokers targeting a virtual property declared in the base class, which is hidden by a C# field which is then overridden by an aspect.
 */

public class InvokerBeforeAspect : FieldOrPropertyAspect
{
    public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
    {
        builder.OverrideAccessors(
            nameof(GetTemplate),
            nameof(SetTemplate),
            new { target = builder.Target.DeclaringType!.BaseType!.FieldsAndProperties.OfName( "Field" ).Single() } );
    }

    [Template]
    public dynamic? GetTemplate( [CompileTime] IFieldOrProperty target )
    {
        meta.InsertComment( "Invoke TargetClass.Field" );
        _ = target.Value;
        meta.InsertComment( "Invoke TargetClass._field" );
        _ = target.With( InvokerOptions.Base ).Value;
        meta.InsertComment( "Invoke TargetClass._field" );
        _ = target.With( InvokerOptions.Current ).Value;
        meta.InsertComment( "Invoke TargetClass.Field" );
        _ = target.With( InvokerOptions.Final ).Value;

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate( [CompileTime] IFieldOrProperty target )
    {
        meta.InsertComment( "Invoke TargetClass.Field" );
        target.Value = 42;
        meta.InsertComment( "Invoke TargetClass._field" );
        target.With( InvokerOptions.Base ).Value = 42;
        meta.InsertComment( "Invoke TargetClass._field" );
        target.With( InvokerOptions.Current ).Value = 42;
        meta.InsertComment( "Invoke TargetClass.Field" );
        target.With( InvokerOptions.Final ).Value = 42;

        meta.Proceed();
    }
}

public class OverrideAspect : FieldOrPropertyAspect
{
    public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
    {
        builder.OverrideAccessors( nameof(GetTemplate), nameof(SetTemplate) );
    }

    [Template]
    public dynamic? GetTemplate()
    {
        meta.InsertComment( "Invoke TargetClass._field" );
        _ = meta.Target.FieldOrProperty.Value;
        meta.InsertComment( "Invoke TargetClass._field" );
        _ = meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value;
        meta.InsertComment( "Invoke TargetClass.Field" );
        _ = meta.Target.FieldOrProperty.With( InvokerOptions.Current ).Value;
        meta.InsertComment( "Invoke TargetClass.Field" );
        _ = meta.Target.FieldOrProperty.With( InvokerOptions.Final ).Value;
        meta.InsertComment( "Invoke TargetClass._field" );

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate()
    {
        meta.InsertComment( "Invoke TargetClass._field" );
        meta.Target.FieldOrProperty.Value = 42;
        meta.InsertComment( "Invoke TargetClass._field" );
        meta.Target.FieldOrProperty.With( InvokerOptions.Base ).Value = 42;
        meta.InsertComment( "Invoke TargetClass.Field" );
        meta.Target.FieldOrProperty.With( InvokerOptions.Current ).Value = 42;
        meta.InsertComment( "Invoke TargetClass.Field" );
        meta.Target.FieldOrProperty.With( InvokerOptions.Final ).Value = 42;
        meta.InsertComment( "Invoke TargetClass._field" );
        meta.Proceed();
    }
}

public class InvokerAfterAspect : FieldOrPropertyAspect
{
    public override void BuildAspect( IAspectBuilder<IFieldOrProperty> builder )
    {
        builder.OverrideAccessors(
            nameof(GetTemplate),
            nameof(SetTemplate),
            new { target = builder.Target.DeclaringType!.BaseType!.FieldsAndProperties.OfName( "Field" ).Single() } );
    }

    [Template]
    public dynamic? GetTemplate( [CompileTime] IFieldOrProperty target )
    {
        meta.InsertComment( "Invoke TargetClass.Field" );
        _ = target.Value;
        meta.InsertComment( "Invoke TargetClass.Field" );
        _ = target.With( InvokerOptions.Base ).Value;
        meta.InsertComment( "Invoke TargetClass.Field" );
        _ = target.With( InvokerOptions.Current ).Value;
        meta.InsertComment( "Invoke TargetClass.Field" );
        _ = target.With( InvokerOptions.Final ).Value;
        meta.InsertComment( "Invoke TargetClass.Field" );

        return meta.Proceed();
    }

    [Template]
    public void SetTemplate( [CompileTime] IFieldOrProperty target )
    {
        meta.InsertComment( "Invoke TargetClass.Field" );
        target.Value = 42;
        meta.InsertComment( "Invoke TargetClass.Field" );
        target.With( InvokerOptions.Base ).Value = 42;
        meta.InsertComment( "Invoke TargetClass.Field" );
        target.With( InvokerOptions.Current ).Value = 42;
        meta.InsertComment( "Invoke TargetClass.Field" );
        target.With( InvokerOptions.Final ).Value = 42;

        meta.Proceed();
    }
}

public class BaseClass
{
    public static int Field;
}

// <target>
public class TargetClass : BaseClass
{
    [OverrideAspect]
    public new static int Field;

    [InvokerBeforeAspect]
    public int InvokerBefore
    {
        get
        {
            return 0;
        }
        set { }
    }

    [InvokerAfterAspect]
    public int InvokerAfter
    {
        get
        {
            return 0;
        }
        set { }
    }
}