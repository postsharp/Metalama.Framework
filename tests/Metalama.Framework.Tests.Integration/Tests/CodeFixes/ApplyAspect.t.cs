// Warning MY001 on `Method`: `Apply Aspect2`
//    CodeFix: Apply`
internal class TargetCode
{
  [Aspect1]
  private int Method(int a)
  {
    Console.WriteLine("Oops");
    return a + 1;
  }
}