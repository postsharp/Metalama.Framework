public class TargetClass
{
  public void Method()
  {
  }
  [InvokerAspect]
  public void Invoker(TargetClass instance)
  {
    // Invoke instance.Method
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.TargetClass_DifferentInstance.TargetClass)instance).Method();
    // Invoke instance?.Method
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.TargetClass_DifferentInstance.TargetClass)instance)?.Method();
    // Invoke instance.Method
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.TargetClass_DifferentInstance.TargetClass)instance).Method();
    // Invoke instance?.Method
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.TargetClass_DifferentInstance.TargetClass)instance)?.Method();
    return;
  }
}