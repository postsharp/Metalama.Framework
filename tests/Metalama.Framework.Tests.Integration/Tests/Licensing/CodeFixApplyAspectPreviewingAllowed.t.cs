// --- CodeFixApplyAspectPreviewingAllowed.cs ---
// Warning MY001 on `Method`: `Implement manually`
//    CodeFix: Apply MyAspect to int TargetCode.Method(int a)`
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixApplyAspectPreviewingAllowed;
class Dummy
{
}
// --- _CodeFixApplyAspect.cs ---
// Warning MY001 on `Method`: `Implement manually`
//    CodeFix: Apply MyAspect to int TargetCode.Method(int a)`
internal class TargetCode
{
  // TODO: Check with [SuggestManualImplementationAttribute]; This comment is added with the attribute.
  [SuggestManualImplementationAttribute]
  private int Method(int a)
  {
    Console.WriteLine("This line was implemented using application of and aspect using a code fix.");
    return a;
  }
}