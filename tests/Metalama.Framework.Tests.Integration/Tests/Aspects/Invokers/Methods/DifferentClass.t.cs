public class TargetClass
{
  [InvokerAspect]
  public void Invoker(DifferentClass instance)
  {
    // Invoke instance.Method
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.DifferentClass.DifferentClass)instance).Method();
    // Invoke instance?.Method
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.DifferentClass.DifferentClass)instance)?.Method();
    // Invoke instance.Method
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.DifferentClass.DifferentClass)instance).Method();
    // Invoke instance?.Method
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.DifferentClass.DifferentClass)instance)?.Method();
    return;
  }
}