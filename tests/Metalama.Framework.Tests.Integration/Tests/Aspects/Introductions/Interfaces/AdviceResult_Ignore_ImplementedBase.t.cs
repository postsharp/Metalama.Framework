[Introduction]
public class TargetClass : IBaseInterface, global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IInterface
{
  public void BaseMethod()
  {
  }
  public int BaseProperty { get; set; }
  public event EventHandler? BaseEvent;
  public void Witness()
  {
    global::System.Console.WriteLine("InterfaceType: IInterface, Action: Implement");
    global::System.Console.WriteLine("InterfaceType: IBaseInterface, Action: Ignore");
  }
  public global::System.Int32 Property { get; set; }
  public void Method()
  {
  }
  public event global::System.EventHandler? Event;
}