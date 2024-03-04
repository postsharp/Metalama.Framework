[Override]
public record class TargetClass
{
  private global::System.Int32 X { get; init; }
  private global::System.Int32 Y { get; init; }
  public void Deconstruct(out int X, out int Y)
  {
    X = this.X;
    Y = this.Y;
  }
  private TargetClass(int X, int Y)
  {
    this.X = X;
    this.Y = Y;
    global::System.Console.WriteLine("This is the override.");
    global::System.Console.WriteLine($"Param X = {X}");
    global::System.Console.WriteLine($"Param Y = {Y}");
  }
}