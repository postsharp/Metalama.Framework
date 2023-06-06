[Introduction]
public class TargetClass : global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IInterface, global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Introductions.Interfaces.IBaseInterface
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
  private event EventHandler? _baseEvent;
  public event EventHandler? BaseEvent
  {
    add
    {
      this._baseEvent += value;
    }
    remove
    {
      this._baseEvent -= value;
    }
  }
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
  private event EventHandler? _event;
  public event EventHandler? Event
  {
    add
    {
      this._event += value;
    }
    remove
    {
      this._event -= value;
    }
  }
  public void Witness()
  {
    global::System.Console.WriteLine("Interface: IInterface, Action: Implement");
    global::System.Console.WriteLine("Interface: IBaseInterface, Action: Implement");
    global::System.Console.WriteLine("Member: IInterface.Method(), Action: Override, Target: TargetClass.Method()");
    global::System.Console.WriteLine("Member: IInterface.Property, Action: Override, Target: TargetClass.Property");
    global::System.Console.WriteLine("Member: IInterface.Event, Action: Override, Target: TargetClass.Event");
    global::System.Console.WriteLine("Member: IBaseInterface.BaseMethod(), Action: Override, Target: TargetClass.BaseMethod()");
    global::System.Console.WriteLine("Member: IBaseInterface.BaseProperty, Action: Override, Target: TargetClass.BaseProperty");
    global::System.Console.WriteLine("Member: IBaseInterface.BaseEvent, Action: Override, Target: TargetClass.BaseEvent");
  }
}