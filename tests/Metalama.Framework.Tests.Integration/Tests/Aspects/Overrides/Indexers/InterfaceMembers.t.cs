[Override]
public interface Interface
{
  public int this[int i]
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