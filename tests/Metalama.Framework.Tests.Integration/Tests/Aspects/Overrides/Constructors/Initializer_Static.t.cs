[Override]
public class TargetClass
{
  static TargetClass()
  {
    global::System.Console.WriteLine("This is the initializer 1.");
    global::System.Console.WriteLine("This is the initializer 2.");
    global::System.Console.WriteLine("This is the override 2.");
    global::System.Console.WriteLine("This is the override 1.");
    Console.WriteLine($"This is the original constructor.");
  }
}