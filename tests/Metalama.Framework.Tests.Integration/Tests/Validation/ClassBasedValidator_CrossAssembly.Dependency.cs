#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using Metalama.Framework.Fabrics;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Tests.Validation.ClassBasedValidato_CrossAssembly;

public class TheValidator : InboundReferenceValidator
{
    private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
        new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'." );

    public override void ValidateReferences( ReferenceValidationContext context )
    {
        context.Diagnostics.Report( x => _warning.WithArguments( ( x.ReferenceKind, x.OriginDeclaration ) ) );
    }

    public override ReferenceGranularity Granularity => ReferenceGranularity.ParameterOrAttribute;

    public override ReferenceKinds ValidatedReferenceKinds => ReferenceKinds.All;
}

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectMany( p => p.Types ).ValidateInboundReferences( new TheValidator() );
    }
}

public class C { }