using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

#pragma warning disable CS0168, CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Validation.CrossAssembly
{
    internal class Aspect : TypeAspect
    {
        private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
            new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'." );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder
                .Outbound
                .ValidateOutboundReferences( Validate, ReferenceGranularity.Declaration, ReferenceKinds.All );
        }

        private static void Validate( ReferenceValidationContext context )
        {
            context.Diagnostics.Report( x => _warning.WithArguments( ( x.ReferenceKinds, x.ReferencingDeclaration ) ) );
        }
    }

    [Aspect]
    public class ValidatedClass
    {
        public static void Method( object o ) { }
    }
}