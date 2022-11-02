using System;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.CodeFixes;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixRedistribution.Dependency;

namespace Metalama.Framework.Tests.Integration.Tests.Licensing.CodeFixRedistribution;

// <target>
internal class TargetCode
{
    // TODO: Check with [SuggestMyAttributeRedistributable]; This comment is added with the attribute.
    [SuggestMyAttributeRedistributableAttribute]
    private int Method(int a)
    {
        return a;
    }
}
