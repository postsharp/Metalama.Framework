// --- PartialMethod_SyntaxTrees.1.cs ---
internal partial class TargetClass
{
  public partial int TargetMethod()
  {
    global::System.Console.WriteLine("This is the override of TargetMethod.");
    Console.WriteLine("This is a partial method.");
    return 42;
  }
  partial void TargetVoidMethodWithImplementation()
  {
    global::System.Console.WriteLine("This is the override of TargetVoidMethodWithImplementation.");
    Console.WriteLine("This is a partial method.");
    return;
  }
}
// --- PartialMethod_SyntaxTrees.cs ---
[Override]
internal partial class TargetClass
{
  public partial int TargetMethod();
  partial void TargetMethodNoImplementation();
  partial void TargetMethodNoImplementation()
  {
    global::System.Console.WriteLine("This is the override of TargetMethodNoImplementation.");
    return;
  }
  partial void TargetVoidMethodWithImplementation();
}