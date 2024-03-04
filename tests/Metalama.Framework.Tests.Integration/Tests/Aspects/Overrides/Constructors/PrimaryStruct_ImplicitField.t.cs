[Override]
public struct TargetStruct
{
  public int Foo() => x;
  public int Bar() => y;
  private readonly global::System.Int32 x;
  private readonly global::System.Int32 y;
  private TargetStruct(int x, int y)
  {
    this.x = x;
    this.y = y;
    global::System.Console.WriteLine("This is the override.");
    global::System.Console.WriteLine($"Param x = {x}");
    global::System.Console.WriteLine($"Param y = {y}");
  }
  public TargetStruct()
  {
    global::System.Console.WriteLine("This is the override.");
  }
}