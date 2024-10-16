public class TargetClass
{
  public int Property
  {
    get
    {
      return 0;
    }
    set
    {
    }
  }
  private TargetClass? instance;
  [InvokerAspect]
  public int Invoker
  {
    get
    { // Invoke instance.Property
      _ = ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance.TargetClass)this.instance!).Property;
      // Invoke instance?.Property
      _ = ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance.TargetClass)this.instance!)?.Property;
      // Invoke instance.Property
      _ = ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance.TargetClass)this.instance!).Property;
      // Invoke instance?.Property
      _ = ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance.TargetClass)this.instance!)?.Property;
      return 0;
    }
    set
    { // Invoke instance.Property
      ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance.TargetClass)this.instance!).Property = 42;
      // Invoke instance.Property
      ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Properties.TargetClass_DifferentInstance.TargetClass)this.instance!).Property = 42;
    }
  }
}