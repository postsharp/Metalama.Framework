// --- InstanceConstructing_MultipleSyntaxTrees.1.cs ---
public partial class TargetCode
{
  public TargetCode(int x)
  {
    global::System.Console.WriteLine("TargetCode: Aspect");
  }
  public void Bar()
  {
  }
}
// --- InstanceConstructing_MultipleSyntaxTrees.2.cs ---
public partial class TargetCode
{
}
// --- InstanceConstructing_MultipleSyntaxTrees.cs ---
[Aspect]
public partial class TargetCode
{
  public TargetCode()
  {
    global::System.Console.WriteLine("TargetCode: Aspect");
  }
  private int Method(int a)
  {
    return a;
  }
}