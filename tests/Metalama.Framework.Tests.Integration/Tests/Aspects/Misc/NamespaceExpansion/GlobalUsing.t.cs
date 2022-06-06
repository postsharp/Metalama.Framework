internal class TargetClass
{
    [Override]
    public void TargetMethod_Void()
    {
        global::Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.GlobalUsing.MyNamespaceGoingGlobal.MyClassGoingGlobal.MyMethod();
        Console.WriteLine("This is the original method.");
        return;
    }
}