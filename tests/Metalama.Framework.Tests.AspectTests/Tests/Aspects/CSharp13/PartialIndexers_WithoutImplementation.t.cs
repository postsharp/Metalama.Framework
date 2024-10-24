// Final Compilation.Emit failed.
// Error CS0161 on `get`: `'Target.this[int].get': not all code paths return a value`
// Error CS0161 on `get`: `'Target.this[long].get': not all code paths return a value`
partial class Target
{
  [TheAspect]
  partial int this[int i] { get; set; }
  partial int this[int i]
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
  partial int this[long i] { get; }
  partial int this[long i]
  {
    get
    {
      global::System.Console.WriteLine("This is aspect code.");
    }
  }
}