[Aspect1]
[Aspect2]
abstract class TargetCode
{
  public int Property { get; }
  private TargetCode()
  {
    Property = 42;
    Property = 13;
  }
}