public class TargetClass
{
  private DifferentClass? instance;
  [InvokerAspect]
  public event System.EventHandler Invoker
  {
    add
    { // Invoke instance.Event
      this.instance.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.DifferentClass.TargetClass.StaticTarget;
      // Invoke instance.Event
      this.instance.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.DifferentClass.TargetClass.StaticTarget;
    }
    remove
    { // Invoke instance.Event
      this.instance.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.DifferentClass.TargetClass.StaticTarget;
      // Invoke instance.Event
      this.instance.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.DifferentClass.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
}