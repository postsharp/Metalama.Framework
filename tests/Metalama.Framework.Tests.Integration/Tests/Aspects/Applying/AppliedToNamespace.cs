using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Tests.Integration.Tests.Aspects.Applying.AppliedToNamespace;

[assembly: AspectOrder( typeof(MyTypeAspect), typeof(MyNamespaceAspect) )]

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Applying.AppliedToNamespace;

public class Fabric : NamespaceFabric
{
    public override void AmendNamespace( INamespaceAmender amender )
    {
        amender.Amend.AddAspect<MyNamespaceAspect>();
    }
}

public class MyNamespaceAspect : IAspect<INamespace>
{
    public void BuildAspect( IAspectBuilder<INamespace> builder )
    {
        builder.Amend.SelectMany( ns => ns.Types ).AddAspectIfEligible<MyTypeAspect>();
    }

    public void BuildEligibility( IEligibilityBuilder<INamespace> builder ) { }
}

internal class MyTypeAspect : TypeAspect
{
    [Introduce]
    public void IntroducedMethod() { }
}

// <target>
internal class C { }