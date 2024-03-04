[Override]
public record class TargetClass
{
  public int X;
  public void Foo()
  {
    this.X = 42;
  }
  public void Deconstruct(out int X)
  {
    X = this.X;
  }
  public TargetClass(int X)
  {
    this.X = X;
    global::System.Console.WriteLine("This is the override.");
    global::System.Console.WriteLine($"Param X = {X}");
  }
}