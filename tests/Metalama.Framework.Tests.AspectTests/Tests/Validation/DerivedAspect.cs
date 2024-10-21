using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.AspectTests.Tests.Validation.DerivedAspect;

internal class BaseAspect : TypeAspect
{
    private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration, string SyntaxKind)> _warning =
        new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}' (SyntaxKind={2})." );

    protected static void Validate( ReferenceValidationContext context )
    {
        context.Diagnostics.Report( x => _warning.WithArguments( ( x.ReferenceKind, x.OriginDeclaration, x.Source.Kind ) ) );
    }
}

internal class DerivedAspect : BaseAspect
{
    public override void BuildAspect( IAspectBuilder<INamedType> builder )
    {
        builder
            .Outbound
            .ValidateInboundReferences( Validate, ReferenceGranularity.ParameterOrAttribute, ReferenceKinds.All );
    }
}

// <target>
[DerivedAspect]
internal class C { }

// <target>
internal class D : C { }