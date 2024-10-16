using Metalama.Framework.Tests.AspectTests.Tests.Licensing.CodeFixRedistribution.Dependency;

namespace Metalama.Framework.Tests.AspectTests.Tests.Licensing.CodeFixRedistribution;

// <target>
internal class TargetCode
{
    [SuggestMyAttributeRedistributableAttribute]
    private int Method(int a)
    {
        return a;
    }
}
