[IntroductionAspect]
public class TargetClass : BaseClass
{
  [InvokerBeforeAspect]
  public event System.EventHandler InvokerBefore
  {
    add
    { // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
    }
    remove
    { // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
    }
  }
  [InvokerAfterAspect]
  public event System.EventHandler InvokerAfter
  {
    add
    { // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
    }
    remove
    { // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
  public new event global::System.EventHandler Event
  {
    add
    {
      // Invoke base.Event
      base.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event += value;
    }
    remove
    {
      // Invoke base.Event
      base.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_AspectHidden.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event -= value;
    }
  }
}