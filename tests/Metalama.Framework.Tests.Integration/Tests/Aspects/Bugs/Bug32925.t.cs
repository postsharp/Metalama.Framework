public class C
{
    [TheAspect]
    public void M()
    {
        object result = null;
        global::Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug32925.ExtensionClass.ExtensionMethod(result);
        return;
    }
}
