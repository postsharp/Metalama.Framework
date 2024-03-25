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
