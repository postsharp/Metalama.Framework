[NotNullCheck]
public record SomeRecord(int X, int Y, string Z)
{
  private readonly string _z = Z;
  public string Z
  {
    get
    {
      global::System.String returnValue;
      returnValue = this._z;
      global::System.Console.WriteLine("Aspect");
      return returnValue;
    }
    init
    {
      global::System.Console.WriteLine("Aspect");
      this._z = value;
    }
  }
}