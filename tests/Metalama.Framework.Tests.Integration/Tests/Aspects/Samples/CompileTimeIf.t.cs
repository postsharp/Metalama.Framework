internal class TargetCode
{
  [CompileTimeIf]
  public void InstanceMethod()
  {
    global::System.Console.WriteLine($"Invoking TargetCode.InstanceMethod() on instance {this.ToString()}.");
    Console.WriteLine("InstanceMethod");
    return;
  }
  [CompileTimeIf]
  public static void StaticMethod()
  {
    global::System.Console.WriteLine("Invoking TargetCode.StaticMethod()");
    Console.WriteLine("StaticMethod");
    return;
  }
}