public interface Interface
{
  [Override]
  public static virtual int StaticVirtualMethod()
  {
    global::System.Console.WriteLine("Override.");
    return 42;
  }
}
public class TargetClass : Interface
{
}