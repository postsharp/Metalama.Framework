[Aspect]
public record TargetRecord
{
  private int Method(int a)
  {
    return a;
  }
  public void Deconstruct()
  {
  }
  private TargetRecord()
  {
    global::System.Console.WriteLine("TargetRecord: Aspect");
  }
}