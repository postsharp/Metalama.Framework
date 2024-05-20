internal class TargetClass
{
  private int TargetMethod(int a)
  {
    Console.WriteLine("TargetClass.TargetMethod(int) enhanced by resdistributed TestAspect");
    return a;
  }
}