[TestAspect]
public class Target
{
  public int _field;
  public int GetAutoProperty { get; }
  public int InitAutoProperty { get; init; }
  private int _autoProperty;
  public int AutoProperty
  {
    get
    {
      return this._autoProperty;
    }
    set
    {
      if (value != this._autoProperty)
      {
        this._autoProperty = value;
      }
    }
  }
  public int Property
  {
    get
    {
      return this.Property_Source;
    }
    set
    {
      if (value != this.Property_Source)
      {
        this.Property_Source = value;
      }
    }
  }
  private int Property_Source { get => _field; set => _field = value; }
}