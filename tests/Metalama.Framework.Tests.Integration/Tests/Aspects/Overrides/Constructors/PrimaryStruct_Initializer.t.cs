// Warning CS8618 on `TargetStruct`: `Non-nullable event 'C' must contain a non-null value when exiting constructor. Consider declaring the event as nullable.`
[Override]
public struct TargetStruct
{
  private int a;
  private int B { get; }
  private event EventHandler C;
  public TargetStruct(int x, int y, EventHandler z)
  {
    this.a = x;
    this.B = y;
    this.C = z;
    global::System.Console.WriteLine("This is the override.");
    global::System.Console.WriteLine($"Param x = {x}");
    global::System.Console.WriteLine($"Param y = {y}");
    global::System.Console.WriteLine($"Param z = {z}");
  }
  public TargetStruct()
  {
    global::System.Console.WriteLine("This is the override.");
  }
}