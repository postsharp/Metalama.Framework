[Override]
public record class TargetClass : BaseClass
{
  public global::System.Int32 X { get; init; }
  public global::System.Int32 Y { get; init; }
  public void Deconstruct(out int X, out int Y)
  {
    X = this.X;
    Y = this.Y;
  }
  public TargetClass(int X, int Y) : base(X)
  {
    this.X = X;
    this.Y = Y;
    global::System.Console.WriteLine("This is the override.");
    global::System.Console.WriteLine($"Param X = {X}");
    global::System.Console.WriteLine($"Param Y = {Y}");
  }
}