internal class TargetClass
{
  private EventHandler? _field;
  [Test]
  public event EventHandler Event
  {
    add
    {
      this.Event_Source += value;
    }
    remove
    {
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
      this._eventField += value;
    }
    remove
    {
      this._eventField -= value;
    }
  }
}