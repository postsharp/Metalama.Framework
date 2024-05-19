// --- PartialType_SyntaxTrees.1.cs ---
internal partial class TargetClass
{
  public event EventHandler TargetEvent2
  {
    add
    {
      global::System.Console.WriteLine("This is the override of TargetEvent2.");
      Console.WriteLine("This is TargetEvent2.");
      return;
    }
    remove
    {
      global::System.Console.WriteLine("This is the override of TargetEvent2.");
      Console.WriteLine("This is TargetEvent2.");
      return;
    }
  }
}
// --- PartialType_SyntaxTrees.2.cs ---
internal partial class TargetClass
{
  public event EventHandler TargetEvent3
  {
    add
    {
      global::System.Console.WriteLine("This is the override of TargetEvent3.");
      Console.WriteLine("This is TargetEvent3.");
      return;
    }
    remove
    {
      global::System.Console.WriteLine("This is the override of TargetEvent3.");
      Console.WriteLine("This is TargetEvent3.");
      return;
    }
  }
}
// --- PartialType_SyntaxTrees.cs ---
[Override]
internal partial class TargetClass
{
  public event EventHandler TargetEvent1
  {
    add
    {
      global::System.Console.WriteLine("This is the override of TargetEvent1.");
      Console.WriteLine("This is TargetEvent1.");
      return;
    }
    remove
    {
      global::System.Console.WriteLine("This is the override of TargetEvent1.");
      Console.WriteLine("This is TargetEvent1.");
      return;
    }
  }
}