using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Tests.Integration.Validation.Aspect_AfterAllAspects;
using Metalama.Framework.Validation;

#pragma warning disable CS0168, CS8618, CS0169

[assembly: AspectOrder( typeof(ValidateAspect), typeof(IntroduceAspect) )]

namespace Metalama.Framework.Tests.Integration.Validation.Aspect_AfterAllAspects
{
    internal class ValidateAspect : TypeAspect
    {
        private static readonly DiagnosticDefinition<IDeclaration> _warning =
            new( "MY001", Severity.Warning, "On '{0}'." );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.Amend.SelectMany( t => t.Methods ).AfterAllAspects().Validate( Validate );
        }

        private static void Validate( in DeclarationValidationContext context )
        {
            context.Diagnostics.Report( _warning.WithArguments( context.Declaration ) );
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