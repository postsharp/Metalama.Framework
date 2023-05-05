public class TargetClass
{
  public event System.EventHandler Event
  {
    add
    {
    }
    remove
    {
    }
  }
  private TargetClass? instance;
  [InvokerAspect]
  public event System.EventHandler Invoker
  {
    add
    { // Invoke instance.Event
      ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance.TargetClass)this.instance!).Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance.TargetClass.StaticTarget;
      // Invoke instance.Event
      ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance.TargetClass)this.instance!).Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance.TargetClass.StaticTarget;
    }
    remove
    { // Invoke instance.Event
      ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance.TargetClass)this.instance!).Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance.TargetClass.StaticTarget;
      // Invoke instance.Event
      ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance.TargetClass)this.instance!).Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
}