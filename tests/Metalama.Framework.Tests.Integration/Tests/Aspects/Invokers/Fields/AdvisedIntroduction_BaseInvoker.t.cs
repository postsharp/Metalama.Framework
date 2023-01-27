[Introduction]
[Override]
internal class TargetClass
{
  private global::System.Int32 _field;
  public global::System.Int32 Field
  {
    get
    {
      global::System.Console.WriteLine("Override");
      return this._field;
    }
    set
    {
      global::System.Console.WriteLine("Override");
      this._field = value;
    }
  }
}