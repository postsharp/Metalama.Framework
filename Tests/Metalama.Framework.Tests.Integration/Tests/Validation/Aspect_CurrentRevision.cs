using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Tests.Integration.Validation.Aspect_CurrentRevision;
using Metalama.Framework.Validation;
using System.Diagnostics;

#pragma warning disable CS0168, CS8618, CS0169

[assembly: AspectOrder( typeof(IntroduceAspect), typeof(ValidateAspect) )]

namespace Metalama.Framework.Tests.Integration.Validation.Aspect_CurrentRevision
{
    internal class ValidateAspect : TypeAspect
    {
        private static readonly DiagnosticDefinition<IDeclaration> _warning =
            new( "MY001", Severity.Warning, "On '{0}'." );

        public override void BuildAspect( IAspectBuilder<INamedType> builder )
        {
            builder.WithTargetMembers( t => t.Methods ).Validate( Validate );
        }

        private static void Validate( in DeclarationValidationContext context )
        {
            Debugger.Break();
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