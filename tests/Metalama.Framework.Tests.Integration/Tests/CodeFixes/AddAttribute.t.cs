// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
internal class TargetCode
{
  [Aspect]
  [My]
  private int Method(int a)
  {
    return a;
  }
}