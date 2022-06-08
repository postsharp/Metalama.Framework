internal class C
{
    [return: MyAspect]
    [return: global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_ReturnValue.MyAttribute]    private void M(int p) { }
}