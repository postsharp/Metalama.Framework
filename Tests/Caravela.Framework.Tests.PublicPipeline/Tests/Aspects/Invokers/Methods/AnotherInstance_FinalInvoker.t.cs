[TestAttribute]
    internal class TargetClass
    {
        public void Foo()
{
    var x = this;
    ((global::Caravela.Framework.IntegrationTests.Aspects.Invokers.Events.AnotherInstance_FinalInvoker.TargetClass)x).Foo();
    return;
}
    }