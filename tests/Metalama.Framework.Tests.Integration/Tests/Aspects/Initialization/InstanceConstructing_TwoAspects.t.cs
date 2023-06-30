[FirstAspect]
[SecondAspect]
public class TargetCode
{
  public TargetCode()
  {
    global::System.Console.WriteLine("TargetCode: SecondAspect Second");
    global::System.Console.WriteLine("TargetCode: FirstAspect First");
  }
  public TargetCode(int x)
  {
    global::System.Console.WriteLine("TargetCode: SecondAspect Second");
    global::System.Console.WriteLine("TargetCode: FirstAspect First");
  }
  private int Method(int a)
  {
    return a;
  }
}