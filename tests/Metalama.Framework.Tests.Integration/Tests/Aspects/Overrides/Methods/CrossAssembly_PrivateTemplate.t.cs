namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Overrides.Methods.CrossAssembly_PrivateTemplate;
[MyAspect]
internal class C
{
    private void M()
    {
        global::System.Console.WriteLine("Overridden");
        return;
    }
}