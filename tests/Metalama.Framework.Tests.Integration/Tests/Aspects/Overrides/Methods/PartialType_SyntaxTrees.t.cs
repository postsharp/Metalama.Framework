// --- PartialType_SyntaxTrees.1.cs ---
internal partial class TargetClass
{
  public void TargetMethod2()
  {
    global::System.Console.WriteLine("This is the override of TargetMethod2.");
    Console.WriteLine("This is TargetMethod2.");
    return;
  }
}
// --- PartialType_SyntaxTrees.2.cs ---
internal partial class TargetClass
{
  public void TargetMethod3()
  {
    global::System.Console.WriteLine("This is the override of TargetMethod3.");
    Console.WriteLine("This is TargetMethod3.");
    return;
  }
}
// --- PartialType_SyntaxTrees.cs ---
[Override]
internal partial class TargetClass
{
  public void TargetMethod1()
  {
    global::System.Console.WriteLine("This is the override of TargetMethod1.");
    Console.WriteLine("This is TargetMethod1.");
    return;
  }
}