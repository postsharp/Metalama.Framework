[Introduction]
public class TargetClass : IBaseInterface, global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IInterface
{
  public void BaseMethod()
  {
  }
  private int _baseProperty;
  public int BaseProperty
  {
    get
    {
      return this._baseProperty;
    }
    set
    {
      this._baseProperty = value;
    }
  }
  public event EventHandler? BaseEvent;
  public void Method()
  {
  }
  private int _property;
  public int Property
  {
    get
    {
      return this._property;
    }
    set
    {
      this._property = value;
    }
  }
  public event EventHandler? Event;
  public void Witness()
  {
    global::System.Console.WriteLine("InterfaceType: IInterface, Action: Implement");
    global::System.Console.WriteLine("InterfaceType: IBaseInterface, Action: Implement");
  }
}