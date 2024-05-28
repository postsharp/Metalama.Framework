[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IInterface, global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IBaseInterface
{
  public void BaseMethod()
  {
  }
  public int BaseProperty { get; set; }
  public event EventHandler? BaseEvent;
  public void Method()
  {
  }
  public int Property { get; set; }
  public event EventHandler? Event;
  public void Witness()
  {
    global::System.Console.WriteLine("InterfaceType: IInterface, Action: Implement");
    global::System.Console.WriteLine("InterfaceType: IBaseInterface, Action: Implement");
  }
}