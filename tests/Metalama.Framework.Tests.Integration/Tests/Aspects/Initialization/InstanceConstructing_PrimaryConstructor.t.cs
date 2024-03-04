[Aspect]
public class TargetCode
{
  public int X { get; }
  private TargetCode(int x)
  {
    this.X = x;
    global::System.Console.WriteLine("TargetCode: Aspect");
  }
}