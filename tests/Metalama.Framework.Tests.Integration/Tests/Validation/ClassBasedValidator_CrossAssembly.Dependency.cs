#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using Metalama.Framework.Fabrics;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Tests.Validation.ClassBasedValidato_CrossAssembly;

public class TheValidator : ReferenceValidator
{
    private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
        new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'." );

    public override void Validate( in ReferenceValidationContext context )
    {
        context.Diagnostics.Report( _warning.WithArguments( ( context.ReferenceKinds, context.ReferencingDeclaration ) ) );
    }
}

public class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectMany( p => p.Types ).ValidateReferences( new TheValidator() );
    }
}

public class C { }