public class TargetClass
{
  public void Method()
  {
    global::System.Console.WriteLine();
  }
  [InvokerAspect]
  public void Invoker(TargetClass instance)
  {
    ((global::Metalama.Framework.Tests.Integration.Tests.Aspects.Invokers.Methods.TargetClass_DifferentInstance_Current.TargetClass)instance).Method();
    return;
  }
}