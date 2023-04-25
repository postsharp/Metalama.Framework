public class TargetClass : BaseClass
{
  public new static event System.EventHandler Event
  {
    add
    {
    }
    remove
    {
    }
  }
  [InvokerAspect]
  public event System.EventHandler Invoker
  {
    add
    { // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.BaseClass.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.TargetClass.StaticTarget;
    }
    remove
    { // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.TargetClass.StaticTarget;
      // Invoke BaseClass.Event
      global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.BaseClass.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassStatic_UserHidden.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
}