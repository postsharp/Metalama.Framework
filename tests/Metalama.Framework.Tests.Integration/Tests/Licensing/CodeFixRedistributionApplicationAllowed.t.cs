// --- _CodeFixRedistribution.cs ---
// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
internal class TargetCode
{
  [SuggestMyAttributeRedistributableAttribute]
  [My]
  private int Method(int a)
  {
    return a;
  }
}
// --- CodeFixRedistributionApplicationAllowed.cs ---
// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixRedistributionApplicationAllowed;
class Dummy
{
}