public class TargetClass
{
  [TestAspect(13, 42)]
  public void Foo()
  {
    global::System.Console.WriteLine(13);
    global::System.Console.WriteLine(42);
    Console.WriteLine("Original");
    return;
  }
}