#if TEST_OPTIONS
// @RemoveOutputCode
#endif

using System.Collections.Generic;
using System.Linq;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Validation;

#pragma warning disable CS0168, CS8618, CS0169, CS0219, CS0067

namespace Metalama.Framework.Tests.Integration.Validation.AllReferences_Derived
{
    internal class Aspect : TypeAspect
    {
        private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration, string SyntaxKind)> _warning =
            new( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}' (SyntaxKind={2})." );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder
                .Outbound
                .ValidateReferences( Validate, ReferenceKinds.All, true );
        }

        private static void Validate( in ReferenceValidationContext context )
        {
            context.Diagnostics.Report( _warning.WithArguments( ( context.ReferenceKinds, context.ReferencingDeclaration, context.Source.Kind ) ) );
        }
    }

    [Aspect]
    internal class ValidatedClass { }

    // <target>
    internal class DerivedClass : ValidatedClass
    {
        public static void Method( object o ) { }

        public virtual void VirtualMethod() { }

        public static int StaticField;
        public int InstanceField;
    }

    internal class ReferencingClass
    {
        private void ReferencingMethod()
        {
            // Local variable.
            DerivedClass variable;

            // Typeof.
            DerivedClass.Method( typeof(DerivedClass) );

            // Type argument of generic type.
            List<DerivedClass> list = new();

            // Type argument of generic method.
            _ = new object[0].OfType<DerivedClass>();
        }
    }
}