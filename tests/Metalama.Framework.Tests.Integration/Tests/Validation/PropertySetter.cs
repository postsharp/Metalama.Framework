#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Tests.Validation.PropertySetter
{
    internal class Aspect : MethodAspect
    {
        private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration, string SyntaxKind)> _warning =
            new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}' (SyntaxKind={2})." );

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder
                .Outbound
                .ValidateReferences( Validate, ReferenceKinds.All );
        }

        private static void Validate( in ReferenceValidationContext context )
        {
            context.Diagnostics.Report( _warning.WithArguments( ( context.ReferenceKinds, context.ReferencingDeclaration, context.Source.Kind ) ) );
        }
    }

    internal class C
    {
        public string P
        {
            get;
            [Aspect]
            set;
        } = "";

        public string this[ int i ]
        {
            get => "";

            [Aspect]
            set { }
        }
    }

    internal class D
    {
        public void M()
        {
            var c = new C();

            // There should be NO match in the next line.
            _ = c.P;
            _ = c[5];

            // There should be a match in the next line.
            c.P = "";
            c[5] = "";
            _ = new C { P = "" };
        }
    }
}