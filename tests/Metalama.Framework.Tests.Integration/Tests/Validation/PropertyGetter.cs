#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using System.Diagnostics;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Tests.Validation.PropertyGetter
{
    internal class Aspect : MethodAspect
    {
        private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration, string SyntaxKind)> _warning =
            new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}' (SyntaxKind={2})." );

        public override void BuildAspect( IAspectBuilder<IMethod> builder )
        {
            builder
                .Outbound
                .ValidateOutboundReferences( Validate, ReferenceGranularity.Declaration, ReferenceKinds.All );
        }

        private static void Validate( ReferenceValidationContext context )
        {
            Debugger.Break();
            context.Diagnostics.Report( x => _warning.WithArguments( ( x.ReferenceKinds, x.ReferencingDeclaration, x.Source.Kind ) ) );
        }
    }

    internal class C
    {
        public string P
        {
            [Aspect]
            get;
            set;
        } = "";

        public string this[ int i ]
        {
            [Aspect]
            get => "";
            set { }
        }
    }

    internal class D
    {
        public void M()
        {
            var c = new C();

            // There should be a match in the next lines.
            _ = c.P;
            _ = c[5];

            // There should be NO match in the next lines.
            c.P = "";
            c[5] = "";
            _ = new C { P = "" };
        }
    }
}