[InnerOverride]
[OuterOverride]
public class TargetClass
{
  public TargetClass()
  {
    global::System.Console.WriteLine("This is the inner override.");
    global::System.Console.WriteLine("This is the outer override.");
    Console.WriteLine($"This is the original constructor.");
  }
}