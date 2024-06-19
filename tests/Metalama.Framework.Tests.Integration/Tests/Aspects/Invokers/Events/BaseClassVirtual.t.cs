public class TargetClass : BaseClass
{
  [InvokerAspect]
  public event EventHandler Invoker
  {
    add
    { // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event += global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual.TargetClass.StaticTarget;
    }
    remove
    { // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual.TargetClass.StaticTarget;
      // Invoke base.Event
      base.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual.TargetClass.StaticTarget;
      // Invoke this.Event
      this.Event -= global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Events.BaseClassVirtual.TargetClass.StaticTarget;
    }
  }
  public static void StaticTarget(object? sender, EventArgs args)
  {
  }
}