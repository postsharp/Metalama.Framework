public class TargetClass : BaseClass
{
  public override void Foo()
  {
    global::System.Console.WriteLine(42);
    global::System.Console.WriteLine(42);
    global::System.Console.WriteLine(42);
    Console.WriteLine("Original");
    return;
  }
}