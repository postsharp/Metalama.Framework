internal class TargetClass
{
  private int _property;
  [Test]
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
}