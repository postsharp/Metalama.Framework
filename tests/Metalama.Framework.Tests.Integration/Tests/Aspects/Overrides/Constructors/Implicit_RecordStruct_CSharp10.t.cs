[Override]
public record struct TargetStruct
{
  public TargetStruct()
  {
    this = default;
    global::System.Console.WriteLine("This is the override.");
  }
}