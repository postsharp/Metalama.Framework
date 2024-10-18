// Warning MY001 on `Method1`: `Add some attribute`
//    CodeFix: Remove [My] from 'T.TargetCode'`
internal class T
{
  internal partial class TargetCode
  {
    [Aspect]
    private int Method1(int a)
    {
      return a;
    }
    private int Method2(int a)
    {
      return a;
    }
  }
  internal partial class TargetCode
  {
    [Your]
    private int Method3(int a)
    {
      return a;
    }
  }
}