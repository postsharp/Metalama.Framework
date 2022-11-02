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
    [SuggestMyAttributeRedistributableAttribute]
    private int Method(int a)
    {
        return a;
    }
}
