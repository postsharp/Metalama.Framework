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
      _ = this.instance.Property;
      // Invoke instance?.Property
      _ = this.instance?.Property;
      // Invoke instance.Property
      _ = this.instance.Property;
      // Invoke instance?.Property
      _ = this.instance?.Property;
      return 0;
    }
    set
    { // Invoke instance.Property
      ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.FieldsAndProperties.TargetClass_DifferentInstance.TargetClass)this.instance!).Property = 42;
      // Invoke instance.Property
      ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.FieldsAndProperties.TargetClass_DifferentInstance.TargetClass)this.instance!).Property = 42;
    }
  }
}