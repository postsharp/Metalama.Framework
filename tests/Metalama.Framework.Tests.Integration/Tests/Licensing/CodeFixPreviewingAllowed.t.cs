// --- CodeFixPreviewingAllowed.cs ---
// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixPreviewingAllowed;
class Dummy
{
}
// --- _CodeFix.cs ---
// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
internal class TargetCode
{
  // TODO: Check with [SuggestMyAttribute]; This comment is added with the attribute.
  [SuggestMyAttributeAttribute]
  // TODO: Check with [SuggestMyAttribute]; This comment is added with the attribute.
  [My]
  private int Method(int a)
  {
    return a;
  }
}