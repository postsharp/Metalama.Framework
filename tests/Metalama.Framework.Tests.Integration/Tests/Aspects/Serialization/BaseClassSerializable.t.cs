public class TargetClass
{
  [TestAspect(13, 27, 42)]
  public void Foo()
  {
    global::System.Console.WriteLine(13);
    global::System.Console.WriteLine(27);
    global::System.Console.WriteLine(42);
    Console.WriteLine("Original");
    return;
  }
}