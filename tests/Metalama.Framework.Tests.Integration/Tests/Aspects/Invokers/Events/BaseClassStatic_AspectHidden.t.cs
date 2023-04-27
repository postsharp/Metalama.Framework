[IntroductionAspect]
public class TargetClass : BaseClass
{
  [InvokerBeforeAspect]
  public event System.EventHandler InvokerBefore
  {
    add
    { // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
    }
    remove
    { // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
    }
  }
  [InvokerAfterAspect]
  public event System.EventHandler InvokerAfter
  {
    add
    { // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
    }
    remove
    { // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
  public static new event global::System.EventHandler Event
  {
    add
    {
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event += value;
    }
    remove
    {
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_AspectHidden.BaseClass.Event -= value;
    }
  }
}