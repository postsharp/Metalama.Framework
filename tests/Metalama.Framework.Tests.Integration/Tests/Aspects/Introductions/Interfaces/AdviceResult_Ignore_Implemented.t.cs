[Introduction]
public class TargetClass : IInterface
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
    global::System.Console.WriteLine("InterfaceType: IInterface, Action: Ignore");
    global::System.Console.WriteLine("InterfaceType: IBaseInterface, Action: Ignore");
  }
}