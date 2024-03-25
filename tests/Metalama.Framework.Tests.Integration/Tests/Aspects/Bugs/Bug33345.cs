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
        class Bar
        {
            void Valid()
            {
                new Foo();
            }

            void Invalid()
            {
                new Foo(false);
                new Foo("name", false);
            }
        }
    }

    namespace Tests
    {
        class Bar
        {
            void Valid()
            {
                new Foo();
                new Foo(true);
                new Foo("name", true);
            }
        }
    }

    class Foo
    {
        public string Name { get; }
        public bool IsTest { get; }

        [ForTestOnly]
        public Foo(string name, bool isTest)
        {
            this.IsTest = isTest;
            this.Name = name;
        }

        [ForTestOnly]
        public Foo(bool isTest) : this("default", isTest) { }

        public Foo(string name = "default") : this(false) { }
    }

    class ForTestOnlyAttribute : Aspect, IAspect<IDeclaration>
    {
        private static readonly DiagnosticDefinition<IDeclaration> _warning = new(
            "DEMO02",
            Severity.Warning,
            "'{0}' can be used only in a namespace whose name ends with '.Tests'");

        public void BuildAspect(IAspectBuilder<IDeclaration> builder)
        {
            builder.Outbound.ValidateReferences(this.ValidateReference, ReferenceKinds.All);
        }

        public void BuildEligibility(IEligibilityBuilder<IDeclaration> builder)
        {
        }

        private void ValidateReference(in ReferenceValidationContext context)
        {
            if ( !context.ReferencingType.Is((INamedType)context.ReferencedDeclaration.ContainingDeclaration!) && !context.ReferencingType.Namespace.FullName.EndsWith(".Tests"))
            {
                context.Diagnostics.Report(_warning.WithArguments(context.ReferencedDeclaration));
            }
        }
    }
}