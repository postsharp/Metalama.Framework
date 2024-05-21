// --- PartialType_SyntaxTrees.1.cs ---
internal partial class TargetClass
{
  public static TargetClass operator -(TargetClass a, TargetClass b)
  {
    global::System.Console.WriteLine("This is the override of op_Subtraction.");
    Console.WriteLine($"This is the original operator.");
    return new TargetClass();
  }
}
// --- PartialType_SyntaxTrees.2.cs ---
internal partial class TargetClass
{
  public static TargetClass operator *(TargetClass a, TargetClass b)
  {
    global::System.Console.WriteLine("This is the override of op_Multiply.");
    Console.WriteLine($"This is the original operator.");
    return new TargetClass();
  }
}
// --- PartialType_SyntaxTrees.cs ---
[Override]
internal partial class TargetClass
{
  public static TargetClass operator +(TargetClass a, TargetClass b)
  {
    global::System.Console.WriteLine("This is the override of op_Addition.");
    Console.WriteLine($"This is the original operator.");
    return new TargetClass();
  }
}