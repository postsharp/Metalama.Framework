#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using Metalama.Framework.Fabrics;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;
using Metalama.Framework.Code;
using System.Text.RegularExpressions;

namespace Metalama.Framework.Tests.Integration.Tests.Validation.ExternalAssembly;

#pragma warning disable CS0169

public class Fabric : ProjectFabric
{
    private static readonly DiagnosticDefinition<string> _warning =
        new( "MY001", Severity.Warning, "Referencing the forbidden assembly." );

    public override void AmendProject( IProjectAmender amender )
    {
        amender.Outbound.SelectMany( compilation => compilation.ReferencedAssemblies.OfName( typeof(Regex).Assembly.GetName().Name! ) )
            .ValidateReferences( Validate, ReferenceKinds.All );
    }

    private void Validate( in ReferenceValidationContext context )
    {
        context.Diagnostics.Report( _warning.WithArguments( ( (IAssembly)context.ReferencedDeclaration ).Identity.Name ) );
    }
}

internal class C
{
    private Regex? _regex;
}