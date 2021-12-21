using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Tests.Integration.Validation.DeclarationValidation;
using Metalama.Framework.Validation;

#pragma warning disable CS0168, CS8618, CS0169

[assembly: AspectOrder( typeof(ValidateAspect), typeof(IntroduceAspect) )]

namespace Metalama.Framework.Tests.Integration.Validation.DeclarationValidation
{
    internal class ValidateAspect : TypeAspect
    {
        private static readonly DiagnosticDefinition<IDeclaration> _warning =
            new( "MY001", Severity.Warning, "On '{0}'." );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.WithTargetMembers( t => t.Methods ).RegisterDeclarationValidator( nameof(Validate) );
        }

        private static void Validate( in DeclarationValidationContext context )
        {
            _warning.WithArguments( context.Declaration ).ReportTo( context.Diagnostics );
        }
    }

    internal class IntroduceAspect : TypeAspect
    {
        [Introduce]
        public void IntroducedMethod() { }
    }

    // <target>
    [ValidateAspect]
    [IntroduceAspect]
    internal class ValidatedClass
    {
        public static void SourceMethod( object o ) { }
    }
}