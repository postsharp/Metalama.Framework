using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Options;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Project;

namespace Metalama.Framework.Tests.Integration.Tests.Options.NonEligible;

public record NonElifibleOptions : IHierarchicalOptions<INamedType>, IEligible<INamedType>
{
#if !NET5_0_OR_GREATER
    public IHierarchicalOptions GetDefaultOptions( OptionsInitializationContext context ) => this;
#endif

    public object ApplyChanges( object changes, in ApplyChangesContext context )
    {
        return this;
    }

    public void BuildEligibility( IEligibilityBuilder<INamedType> builder )
    {
        builder.MustBeStatic();
    }
}

public class C { }

public static class D { }

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.Outbound.SelectMany( c => c.Types ).SetOptions( _ => new NonElifibleOptions() );
    }
}