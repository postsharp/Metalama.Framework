// --- _AspectWeaver.cs ---
internal class TargetCode
{
  // Rewritten.
  [Aspect]
  private int TransformedMethod(int a) => 0;
  private int NotTransformedMethod(int a) => 0;
}
// --- SdkAllowedByProfessional.cs ---
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.SdkAllowedByProfessional;
class Dummy
{
}