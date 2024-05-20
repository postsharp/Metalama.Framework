// --- _CodeFixApplyAspect.cs ---
// Warning MY001 on `Method`: `Implement manually`
//    CodeFix: Apply MyAspect to int TargetCode.Method(int a)`
internal class TargetCode
{
  [SuggestManualImplementationAttribute]
  private int Method(int a)
  {
    Console.WriteLine("This line was implemented using application of and aspect using a code fix.");
    return a;
  }
}
// --- CodeFixApplyAspectPreviewingAllowedProjectBound.cs ---
// Warning MY001 on `Method`: `Implement manually`
//    CodeFix: Apply MyAspect to int TargetCode.Method(int a)`
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixApplyAspectPreviewingAllowedProjectBound;
class Dummy
{
}