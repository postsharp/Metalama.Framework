// --- _CodeFix.cs ---
// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
internal class TargetCode
{
  [SuggestMyAttributeAttribute]
  [My]
  private int Method(int a)
  {
    return a;
  }
}
// --- CodeFixApplicationAllowedProjectBound.cs ---
// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixApplicationAllowedProjectBound;
class Dummy
{
}