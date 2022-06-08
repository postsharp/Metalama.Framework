internal class C
{
    [MyAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]    private eventAction Event1;
    [MyAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]    private eventAction Event2;

    [MyAspect]
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]    private event Action Event3;
    [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]    private event Action Event4
    {
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]        add { }
        [global::Metalama.Framework.Tests.Integration.Tests.Aspects.Attributes.Add_Event.MyAttribute]        remove { }
    }
}