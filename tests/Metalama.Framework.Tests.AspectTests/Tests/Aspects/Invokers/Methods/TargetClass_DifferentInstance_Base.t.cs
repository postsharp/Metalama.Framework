public class TargetClass
{
  public void Method()
  {
    global::System.Console.WriteLine();
  }
  private void Method_Source()
  {
  }
  [InvokerAspect]
  public void Invoker(TargetClass instance)
  {
    ((global::Metalama.Framework.Tests.AspectTests.Tests.Aspects.Invokers.Methods.TargetClass_DifferentInstance_Base.TargetClass)instance).Method_Source();
    return;
  }
}