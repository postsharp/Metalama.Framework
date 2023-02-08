internal class TargetClass
{
  [Test]
  public void VoidMethod()
  {
    global::Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_ConditionalFinalInvoker.TargetClass? local = null;
    local?.VoidMethod();
    return;
  }
  [Test]
  public int? Method(int? x)
  {
    global::Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_ConditionalFinalInvoker.TargetClass? local = null;
    return local?.Method(x);
  }
}