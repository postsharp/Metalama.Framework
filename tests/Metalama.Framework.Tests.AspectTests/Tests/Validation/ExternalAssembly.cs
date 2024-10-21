#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using Metalama.Framework.Fabrics;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Tests.AspectTests.Tests.Validation.ExternalAssembly;

#pragma warning disable CS0169

public class Fabric : ProjectFabric
{
    private static readonly DiagnosticDefinition<string> _warning =
        new( "MY001", Severity.Warning, "Referencing the forbidden assembly." );

    public override void AmendProject( IProjectAmender amender )
    {
        amender.SelectMany( compilation => compilation.ReferencedAssemblies.OfName( typeof(Regex).Assembly.GetName().Name! ) )
            .ValidateInboundReferences( Validate, ReferenceGranularity.ParameterOrAttribute, ReferenceKinds.All );
    }

    private void Validate( ReferenceValidationContext context )
    {
        context.Diagnostics.Report( _warning.WithArguments( ( context.Destination.Assembly.Identity.Name ) ) );
    }
}

internal class C
{
    private Regex? _regex;
}