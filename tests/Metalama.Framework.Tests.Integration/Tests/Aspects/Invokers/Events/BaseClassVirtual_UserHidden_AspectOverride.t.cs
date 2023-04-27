public class TargetClass : BaseClass
{
  [OverrideAspect]
  public new event System.EventHandler Event
  {
    add
    { // Invoke this.Event_Source
      this.Event_Source += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event_Source
      this.Event_Source += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event_Source
      this.Event_Source += value;
    }
    remove
    { // Invoke this.Event_Source
      this.Event_Source -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event_Source
      this.Event_Source -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event_Source
      this.Event_Source -= value;
    }
  }
  private event System.EventHandler Event_Source
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
    { // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event_Source
      this.Event_Source += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event_Source
      this.Event_Source += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
    }
    remove
    { // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event_Source
      this.Event_Source -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event_Source
      this.Event_Source -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
    }
  }
  [InvokerAfterAspect]
  public event System.EventHandler InvokerAfter
  {
    add
    { // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
    // Invoke this.Event
    }
    remove
    { // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual_UserHidden_AspectOverride.TargetClass.StaticTarget;
    // Invoke this.Event
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
}