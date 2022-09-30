internal class C
{
    [MyAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.ParamsConstructor.MyAttribute(new global::System.Int32[]{})]
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.ParamsConstructor.MyAttribute(new global::System.Int32[]{1})]
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.ParamsConstructor.MyAttribute(new global::System.Int32[]{1, 2})]
    private void M() { }
}