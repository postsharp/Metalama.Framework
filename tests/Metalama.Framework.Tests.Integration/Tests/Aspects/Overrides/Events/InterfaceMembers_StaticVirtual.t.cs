public interface Interface
{
  [Override]
  public static virtual event EventHandler StaticVirtualEvent
  {
    add
    {
      global::System.Console.WriteLine("Override.");
      Console.WriteLine("Original.");
    }
    remove
    {
      global::System.Console.WriteLine("Override.");
      Console.WriteLine("Original.");
    }
  }
}
public class TargetClass : Interface
{
}