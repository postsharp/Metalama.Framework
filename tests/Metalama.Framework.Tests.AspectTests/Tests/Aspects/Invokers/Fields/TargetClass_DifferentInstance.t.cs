public class TargetClass
{
  public int Field;
  private TargetClass? instance;
  [InvokerAspect]
  public int Invoker
  {
    get
    { // Invoke instance.Field
      _ = ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Fields.TargetClass_DifferentInstance.TargetClass)this.instance!).Field;
      // Invoke instance?.Field
      _ = ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Fields.TargetClass_DifferentInstance.TargetClass)this.instance!)?.Field;
      // Invoke instance.Field
      _ = ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Fields.TargetClass_DifferentInstance.TargetClass)this.instance!).Field;
      // Invoke instance?.Field
      _ = ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Fields.TargetClass_DifferentInstance.TargetClass)this.instance!)?.Field;
      return 0;
    }
    set
    { // Invoke instance.Field
      ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Fields.TargetClass_DifferentInstance.TargetClass)this.instance!).Field = 42;
      // Invoke instance.Field
      ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Fields.TargetClass_DifferentInstance.TargetClass)this.instance!).Field = 42;
    }
  }
}