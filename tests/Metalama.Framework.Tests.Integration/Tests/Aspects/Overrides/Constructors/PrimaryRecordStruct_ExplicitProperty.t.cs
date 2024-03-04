[Override]
public record struct TargetStruct
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
  public TargetStruct(int X)
  {
    this.X = X;
    global::System.Console.WriteLine("This is the override.");
    global::System.Console.WriteLine($"Param X = {X}");
  }
}