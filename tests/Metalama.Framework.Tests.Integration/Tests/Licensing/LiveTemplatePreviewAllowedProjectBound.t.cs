// --- LiveTemplatePreviewAllowedProjectBound.cs ---
// Error LAMA0800 on ``: `This project uses 1 aspect classes, but only 0 are allowed by your license. For details, use the following command: `metalama license usage details --project LiveTemplatePreviewAllowedProjectBound`.`
// --- _LiveTemplate.cs ---
// Error LAMA0800 on ``: `This project uses 1 aspect classes, but only 0 are allowed by your license. For details, use the following command: `metalama license usage details --project LiveTemplatePreviewAllowedProjectBound`.`
internal class TargetClass
{
  private int TargetMethod(int a)
  {
    Console.WriteLine("TargetClass.TargetMethod(int) enhanced by TestAspect");
    return a;
  }
}