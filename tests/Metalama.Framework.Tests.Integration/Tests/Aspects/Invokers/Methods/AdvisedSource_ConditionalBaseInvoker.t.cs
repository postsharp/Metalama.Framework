internal class TargetClass
{
    [Test]
    public void VoidMethod()
    {
        global::Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_ConditionalBaseInvoker.TargetClass? local = null;
        ((global::Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_ConditionalBaseInvoker.TargetClass)local)?.VoidMethod_Source();
        return;
    }
    private void VoidMethod_Source()
    {
    }
    [Test]
    public int? Method(int? x)
    {
        global::Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_ConditionalBaseInvoker.TargetClass? local = null;
        return ((global::Metalama.Framework.IntegrationTests.Aspects.Invokers.Methods.AdvisedSource_ConditionalBaseInvoker.TargetClass)local)?.Method_Source(x);
    }
    private int? Method_Source(int? x)
    {
        return x;
    }
}