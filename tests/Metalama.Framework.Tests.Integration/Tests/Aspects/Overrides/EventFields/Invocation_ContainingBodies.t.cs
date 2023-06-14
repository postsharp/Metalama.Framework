[Override]
internal class TargetClass
{
  private event EventHandler? _event;
  public event EventHandler? Event
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      this._event += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      this._event -= value;
    }
  }
  private static event EventHandler? _staticEvent;
  public static event EventHandler? StaticEvent
  {
    add
    {
      global::System.Console.WriteLine("This is the add template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_ContainingBodies.TargetClass._staticEvent += value;
    }
    remove
    {
      global::System.Console.WriteLine("This is the remove template.");
      global::Metalama.Framework.Tests.Integration.TestInputs.Aspects.Overrides.EventFields.Invocation_ContainingBodies.TargetClass._staticEvent -= value;
    }
  }
  static TargetClass()
  {
    _staticEvent?.Invoke(null, new EventArgs());
  }
  public TargetClass()
  {
    this._event?.Invoke(this, new EventArgs());
  }
  ~TargetClass()
  {
    this._event?.Invoke(this, new EventArgs());
  }
  public void Foo()
  {
    this._event?.Invoke(this, new EventArgs());
  }
  public static void Bar()
  {
    _staticEvent?.Invoke(null, new EventArgs());
  }
  public int Baz
  {
    get
    {
      this._event?.Invoke(this, new EventArgs());
      return 0;
    }
    init
    {
      this._event?.Invoke(this, new EventArgs());
    }
  }
  public event EventHandler? Quz
  {
    add
    {
      this._event?.Invoke(this, new EventArgs());
    }
    remove
    {
      this._event?.Invoke(this, new EventArgs());
    }
  }
  public static explicit operator int (TargetClass targetClass)
  {
    _staticEvent?.Invoke(null, new EventArgs());
    return 0;
  }
  public static int operator +(TargetClass a, TargetClass b)
  {
    _staticEvent?.Invoke(null, new EventArgs());
    return 0;
  }
  public int this[int index]
  {
    get
    {
      this._event?.Invoke(this, new EventArgs());
      return 0;
    }
    set
    {
      this._event?.Invoke(this, new EventArgs());
    }
  }
}