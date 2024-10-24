// Final Compilation.Emit failed.
// Error CS0161 on `TargetMethod1`: `'TargetClass.TargetMethod1()': not all code paths return a value`
// Error CS0165 on `result`: `Use of unassigned local variable 'result'`
internal partial class TargetClass
{
  [Override1]
  public partial int TargetMethod1();
  public partial int TargetMethod1()
  {
    global::System.Console.WriteLine("This is the override of TargetClass.TargetMethod1().");
  }
  [Override2]
  public partial int TargetMethod2();
  public partial int TargetMethod2()
  {
    global::System.Console.WriteLine("This is the override of TargetClass.TargetMethod2().");
    global::System.Int32 result;
    return (global::System.Int32)result;
  }
  [Override3]
  public partial int TargetMethod3();
  public partial int TargetMethod3()
  {
    global::System.Console.WriteLine("This is the override of TargetClass.TargetMethod3().");
    return default;
  }
}