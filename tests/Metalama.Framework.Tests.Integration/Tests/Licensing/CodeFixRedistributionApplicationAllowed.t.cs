// --- CodeFixRedistributionApplicationAllowed.cs ---
// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixRedistributionApplicationAllowed;
class Dummy
{
}
// --- _CodeFixRedistribution.cs ---
// Warning MY001 on `Method`: `Add some attribute`
//    CodeFix: Add [My] to 'TargetCode.Method(int)'`
internal class TargetCode
{
  // TODO: Check with [SuggestMyAttributeRedistributable]; This comment is added with the attribute.
  [SuggestMyAttributeRedistributableAttribute]
  // TODO: Check with [SuggestMyAttributeRedistributable]; This comment is added with the attribute.
  [CodeFixRedistribution.Dependency.MyAttribute]
  private int Method(int a)
  {
    return a;
  }
}