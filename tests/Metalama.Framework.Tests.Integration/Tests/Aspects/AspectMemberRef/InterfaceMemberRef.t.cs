[Introduce]
    internal class Program:global::Metalama.Framework.IntegrationTests.Aspects.AspectMemberRef.InterfaceMemberRef.IInterface{ 

public void SomeMethod()
{
    Method();
    Property = Property + 1;
    Event += EventHandler;
}

private void EventHandler(global::System.Object? sender, global::System.EventArgs a)
{
}

public void Method()
{
}

public global::System.Int32 Property { get; set; }

public event global::System.EventHandler Event;}