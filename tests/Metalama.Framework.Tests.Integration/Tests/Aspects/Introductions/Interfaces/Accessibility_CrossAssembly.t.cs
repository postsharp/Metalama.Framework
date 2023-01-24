[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.Accessibility_CrossAssembly.IInterface
{
  public global::System.Int32 AutoProperty { get; set; }
  public global::System.Int32 AutoProperty_PrivateSetter { get; private set; }
  public global::System.Int32 Property
  {
    get
    {
      return (global::System.Int32)42;
    }
    set
    {
    }
  }
  public global::System.Int32 Property_ExpressionBody
  {
    get
    {
      return (global::System.Int32)42;
    }
  }
  public global::System.Int32 Property_GetOnly
  {
    get
    {
      return (global::System.Int32)42;
    }
  }
  public global::System.Int32 Property_PrivateSetter
  {
    get
    {
      return (global::System.Int32)42;
    }
    private set
    {
    }
  }
  public void Method()
  {
    global::System.Console.WriteLine("Introduced interface member");
  }
  public event global::System.EventHandler? Event
  {
    add
    {
    }
    remove
    {
    }
  }
  public event global::System.EventHandler? EventField;
}