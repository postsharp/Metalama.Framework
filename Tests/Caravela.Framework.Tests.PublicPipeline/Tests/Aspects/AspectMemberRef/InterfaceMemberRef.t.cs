[Introduce]
    class Program
: global::Caravela.Framework.IntegrationTests.Aspects.AspectMemberRef.InterfaceMemberRef.IInterface    {
    
    
public void SomeMethod()
{
    this.Method();
    this.Property = this.Property + 1;
    this.Event += this.EventHandler;
}
    
private void EventHandler(global::System.Object? sender, global::System.EventArgs a)
{
}
    
public void Method()
{
}
    
public global::System.Int32 Property
{
    get;
    set;
}
    
public event global::System.EventHandler Event;
    }