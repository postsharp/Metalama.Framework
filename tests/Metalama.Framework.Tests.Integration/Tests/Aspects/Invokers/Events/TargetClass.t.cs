public class TargetClass
{
  public event EventHandler Event
  {
    add
    {
    }
    remove
    {
    }
  }
  [InvokerAspect]
  public event EventHandler Invoker
  {
    add
    { // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass.TargetClass.StaticTarget;
    }
    remove
    { // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, EventArgs args)
  {
  }
}