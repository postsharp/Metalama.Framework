#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Validation;

#pragma warning disable CS0168, CS8618, CS0169

namespace Metalama.Framework.Tests.Integration.Validation.CompileTimeCodeNotValidated
{
    public class Fabric : ProjectFabric
    {
        private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
            new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'." );

        public override void AmendProject( IProjectAmender amender )
        {
            amender.SelectMany( compilation => compilation.Types )
                .ValidateOutboundReferences( Validate, ReferenceGranularity.ParameterOrAttribute, ReferenceKinds.All );

            // This reference is legal.
            _ = typeof(SomeClass);
        }

        private static void Validate( ReferenceValidationContext context )
        {
            context.Diagnostics.Report( x => _warning.WithArguments( ( x.ReferenceKinds, x.ReferencingDeclaration ) ) );
        }
    }

    internal class SomeClass { }

    internal class ReferencingClass
    {
        private void ReferencingMethod()
        {
            _ = new SomeClass();
        }
    }
}