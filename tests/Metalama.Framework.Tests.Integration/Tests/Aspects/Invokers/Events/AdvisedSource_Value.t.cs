internal class TargetClass
{
  private EventHandler? _field;
  [Test]
  public event EventHandler Event
  {
    add
    {
      global::System.Console.WriteLine("Override");
      this.Event_Source += value;
    }
    remove
    {
      global::System.Console.WriteLine("Override");
      this.Event_Source -= value;
    }
  }
  private event EventHandler Event_Source { add => _field += value; remove => _field -= value; }
  private event EventHandler? _eventField;
  [Test]
  public event EventHandler? EventField
  {
    add
    {
      global::System.Console.WriteLine("Override");
      this._eventField += value;
    }
    remove
    {
      global::System.Console.WriteLine("Override");
      this._eventField -= value;
    }
  }
}