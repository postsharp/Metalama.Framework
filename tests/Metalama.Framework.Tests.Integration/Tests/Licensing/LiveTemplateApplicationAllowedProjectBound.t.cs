// --- LiveTemplateApplicationAllowedProjectBound.cs ---
// --- _LiveTemplate.cs ---
internal class TargetClass
{
  private int TargetMethod(int a)
  {
    Console.WriteLine("TargetClass.TargetMethod(int) enhanced by TestAspect");
    return a;
  }
}