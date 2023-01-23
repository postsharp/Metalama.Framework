internal class TargetClass
{
  [Override]
  private int Method()
  {
    void LocalFunction()
    {
      _ = 42;
    }
    LocalFunction();
    return default;
  }
  [Override]
  private int Method_ExpressionBody()
  {
    void LocalFunction()
    {
      _ = 42;
    }
    LocalFunction();
    return default;
  }
}