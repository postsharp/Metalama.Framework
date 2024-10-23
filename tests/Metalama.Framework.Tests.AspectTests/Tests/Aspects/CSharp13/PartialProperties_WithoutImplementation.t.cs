// Final Compilation.Emit failed.
// Error CS0161 on `get`: `'Target.P1.get': not all code paths return a value`
// Error CS0161 on `get`: `'Target.P2.get': not all code paths return a value`
partial class Target
{
  [TheAspect]
  partial int P1 { get; set; }
  partial int P1
  {
    get
    {
      global::System.Console.WriteLine("This is aspect code.");
    }
    set
    {
      global::System.Console.WriteLine("This is aspect code.");
    }
  }
  [TheAspect]
  partial int P2 { get; }
  partial int P2
  {
    get
    {
      global::System.Console.WriteLine("This is aspect code.");
    }
  }
}