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
    {
      ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance_Base.TargetClass)this.instance!).Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance_Base.TargetClass.StaticTarget;
    }
    remove
    {
      ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance_Base.TargetClass)this.instance!).Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.TargetClass_DifferentInstance_Base.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, System.EventArgs args)
  {
  }
}