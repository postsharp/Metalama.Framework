public class TargetClass : BaseClass
{
  [InvokerAspect]
  public event System.EventHandler Invoker
  {
    add
    { // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClass.TargetClass.StaticTarget;
    }
    remove
    { // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClass.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
}