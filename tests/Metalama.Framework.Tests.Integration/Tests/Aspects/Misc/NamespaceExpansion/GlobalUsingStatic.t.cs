internal class TargetClass
{
    [Override]
    public void TargetMethod_Void()
    {
        global::Metalama.Framework.IntegrationTests.Aspects.Misc.NamespaceExpansion.GlobalUsingStatic.MyClassWithStaticMethods.MyMethodGoingGlobal();
        Console.WriteLine("This is the original method.");
        return;
    }
}