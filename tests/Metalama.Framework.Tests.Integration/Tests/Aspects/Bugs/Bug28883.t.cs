internal class TargetCode
{
  private int _property;
  [TestAttribute]
  private int Property
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