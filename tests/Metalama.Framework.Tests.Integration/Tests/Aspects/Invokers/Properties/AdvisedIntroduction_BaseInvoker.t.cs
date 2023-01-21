[Introduction]
[Override]
internal class TargetClass
{
  private global::System.Int32 _property;
  public global::System.Int32 Property
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return this._property;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      this._property = value;
    }
  }
}