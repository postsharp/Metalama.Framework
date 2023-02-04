using System;
using System.Linq;
using Castle.DynamicProxy.Generators;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoPropertyNoOverride_BaseInvoker;
using Metalama.Framework.Code.Invokers;

[assembly:AspectOrder(typeof(After), typeof(Override), typeof(Before))]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Properties.AutoPropertyNoOverride_BaseInvoker;

public class Override : TypeAspect
{
    public override void BuildAspect(IAspectBuilder<INamedType> builder)
    {
        builder.Advice.OverrideAccessors(builder.Target.Properties.OfName("AutoProperty").Single(), nameof(Template), nameof(Template));
        builder.Advice.OverrideAccessors(builder.Target.Properties.OfName("AutoProperty_Static").Single(), nameof(Template), nameof(Template));
    }

    [Template]
    public dynamic? Template()
    {
        Console.WriteLine("Override");
        return meta.Proceed();
    }

    [Introduce]
    public void Introduced()
    {
        _ = meta.Target.Type.Properties.OfName("AutoProperty").Single().GetValue(meta.Base);
        meta.Target.Type.Properties.OfName("AutoProperty").Single().SetValue(meta.Base, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_Static").Single().GetInvoker( InvokerOptions.Before ).GetValue(null);
        meta.Target.Type.Properties.OfName("AutoProperty_Static").Single().GetInvoker( InvokerOptions.Before ).SetValue(null, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_NoOverride").Single().GetValue(meta.Base);
        meta.Target.Type.Properties.OfName("AutoProperty_NoOverride").Single().SetValue(meta.Base, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_Static_NoOverride").Single().GetInvoker( InvokerOptions.Before ).GetValue(null);
        meta.Target.Type.Properties.OfName("AutoProperty_Static_NoOverride").Single().GetInvoker( InvokerOptions.Before ).SetValue(null, 42);
    }
}

public class Before : TypeAspect
{
    [Introduce]
    public void IntroducedBefore()
    {
        _ = meta.Target.Type.Properties.OfName("AutoProperty").Single().GetValue(meta.Base);
        meta.Target.Type.Properties.OfName("AutoProperty").Single().SetValue(meta.Base, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_Static").Single().GetInvoker( InvokerOptions.Before ).GetValue(null);
        meta.Target.Type.Properties.OfName("AutoProperty_Static").Single().GetInvoker( InvokerOptions.Before ).SetValue(null, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_NoOverride").Single().GetValue(meta.Base);
        meta.Target.Type.Properties.OfName("AutoProperty_NoOverride").Single().SetValue(meta.Base, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_Static_NoOverride").Single().GetInvoker( InvokerOptions.Before ).GetValue(null);
        meta.Target.Type.Properties.OfName("AutoProperty_Static_NoOverride").Single().GetInvoker( InvokerOptions.Before ).SetValue(null, 42);
    }
}

public class After : TypeAspect
{
    [Introduce]
    public void IntroducedAfter()
    {
        _ = meta.Target.Type.Properties.OfName("AutoProperty").Single().GetValue(meta.Base);
        meta.Target.Type.Properties.OfName("AutoProperty").Single().SetValue(meta.Base, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_Static").Single().GetInvoker( InvokerOptions.Before ).GetValue(null);
        meta.Target.Type.Properties.OfName("AutoProperty_Static").Single().GetInvoker( InvokerOptions.Before ).SetValue(null, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_NoOverride").Single().GetValue(meta.Base);
        meta.Target.Type.Properties.OfName("AutoProperty_NoOverride").Single().SetValue(meta.Base, 42);
        _ = meta.Target.Type.Properties.OfName("AutoProperty_Static_NoOverride").Single().GetInvoker( InvokerOptions.Before ).GetValue(null);
        meta.Target.Type.Properties.OfName("AutoProperty_Static_NoOverride").Single().GetInvoker( InvokerOptions.Before ).SetValue(null, 42);
    }
}

// <target>
[Before]
[Override]
[After]
public class Target
{
    public int AutoProperty { get; set; }
    public static int AutoProperty_Static { get; set; }
    public int AutoProperty_NoOverride { get; set; }
    public static int AutoProperty_Static_NoOverride { get; set; }
}