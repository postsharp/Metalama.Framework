public class TargetClass
{
  [TestAspect(42)]
  public void Foo()
  {
    global::System.Console.WriteLine(42);
    Console.WriteLine("Original");
    return;
  }
}