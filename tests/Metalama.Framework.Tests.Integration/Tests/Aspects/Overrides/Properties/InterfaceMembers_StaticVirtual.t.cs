public interface Interface
{
  [Override]
  public static virtual int StaticVirtualProperty
  {
    get
    {
      global::System.Console.WriteLine("Override.");
      return 42;
    }
    set
    {
      global::System.Console.WriteLine("Override.");
    }
  }
}
public class TargetClass : Interface
{
}