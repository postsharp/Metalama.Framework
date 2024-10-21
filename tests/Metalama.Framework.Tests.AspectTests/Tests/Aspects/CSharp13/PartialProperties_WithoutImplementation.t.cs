partial class Target
{
  [TheAspect]
  partial int P1 { get; set; }
  private int _p1;
  partial int P1
  {
    get
    {
      global::System.Console.WriteLine("This is aspect code.");
      return this._p1;
    }
    set
    {
      global::System.Console.WriteLine("This is aspect code.");
      this._p1 = value;
    }
  }
}