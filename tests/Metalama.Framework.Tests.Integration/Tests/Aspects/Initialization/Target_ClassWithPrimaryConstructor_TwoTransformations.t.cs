[Aspect]
abstract class TargetCode
{
  public int Property { get; }
  private TargetCode()
  {
    Property = 13;
    Property = 42;
  }
}