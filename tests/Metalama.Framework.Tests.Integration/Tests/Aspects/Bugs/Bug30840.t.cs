[TrackedObject]
public struct TrackedClass
{
  public TrackedClass()
  {
  }
  private int _i1 = default;
  public int i
  {
    get
    {
      return this._i1;
    }
    set
    {
      this._i1 = value;
      global::System.Console.WriteLine("Overridden setter");
      return;
    }
  }
}