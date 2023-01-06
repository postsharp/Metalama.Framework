internal class C
{
  [Override]
  private int Method_ExpressionBody()
  {
    string LocalFunction()
    {
      _ = 42;
      return (global::System.String)"something";
    }
    var s = LocalFunction();
    return (global::System.Int32)s.Length;
  }
  [Override]
  private int Method_BlockBody()
  {
    string LocalFunction()
    {
      _ = 42;
      return (global::System.String)"something";
    }
    var s = LocalFunction();
    return (global::System.Int32)s.Length;
  }
}