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
            builder.WithTarget()
                .RegisterReferenceValidator( Validate, ReferenceKinds.All );
        }

        private static void Validate( in ReferenceValidationContext context )
        {
            _warning.WithArguments( ( context.ReferenceKinds, context.ReferencingDeclaration ) ).ReportTo( context.Diagnostics );
        }
    }

    [Aspect]
    public class ValidatedClass
    {
        public static void Method( object o ) { }
    }
}