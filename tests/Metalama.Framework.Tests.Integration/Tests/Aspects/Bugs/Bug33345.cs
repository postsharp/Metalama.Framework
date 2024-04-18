#if TEST_OPTIONS
// @RemoveOutputCode(true)
#endif

using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Tests.Aspects.Bugs.Bug33345
{
    namespace NotTests
    {
        internal class Bar
        {
            private void Valid()
            {
                new Foo();
            }

            private void Invalid()
            {
                new Foo( false );
                new Foo( "name", false );
            }
        }
    }

    namespace Tests
    {
        internal class Bar
        {
            private void Valid()
            {
                new Foo();
                new Foo( true );
                new Foo( "name", true );
            }
        }
    }

    internal class Foo
    {
        public string Name { get; }

        public bool IsTest { get; }

        [ForTestOnly]
        public Foo( string name, bool isTest )
        {
            IsTest = isTest;
            Name = name;
        }

        [ForTestOnly]
        public Foo( bool isTest ) : this( "default", isTest ) { }

        public Foo( string name = "default" ) : this( false ) { }
    }

    internal class ForTestOnlyAttribute : Aspect, IAspect<IDeclaration>
    {
        private static readonly DiagnosticDefinition<IDeclaration> _warning = new(
            "DEMO02",
            Severity.Warning,
            "'{0}' can be used only in a namespace whose name ends with '.Tests'" );

        public void BuildAspect( IAspectBuilder<IDeclaration> builder )
        {
            builder.Outbound.ValidateOutboundReferences( ValidateReference, ReferenceGranularity.ParameterOrAttribute, ReferenceKinds.All );
        }

        public void BuildEligibility( IEligibilityBuilder<IDeclaration> builder ) { }

        private void ValidateReference( ReferenceValidationContext context )
        {
            if (!context.Referencing.Type.Is( context.Referenced.Type! ) && !context.Referencing.Namespace.FullName.EndsWith( ".Tests" ))
            {
                context.Diagnostics.Report( _warning.WithArguments( context.Referenced.Declaration ) );
            }
        }
    }
}