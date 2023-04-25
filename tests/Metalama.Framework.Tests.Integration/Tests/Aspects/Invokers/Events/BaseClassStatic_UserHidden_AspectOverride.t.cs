public class TargetClass : BaseClass
{
  [OverrideAspect]
  public new static event System.EventHandler Event
  {
    add
    { // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event_Source += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event_Source += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event_Source += value;
    }
    remove
    { // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event_Source -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event_Source -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.Event_Source -= value;
    }
  }
  private static event System.EventHandler Event_Source
  {
    add
    {
    }
    remove
    {
    }
  }
  [InvokerBeforeAspect]
  public event System.EventHandler InvokerBefore
  {
    add
    { // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
    }
    remove
    { // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event_Source
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
    }
  }
  [InvokerAfterAspect]
  public event System.EventHandler InvokerAfter
  {
    add
    { // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
    // Invoke TargetClass.Event
    }
    remove
    { // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke TargetClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden_AspectOverride.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
}