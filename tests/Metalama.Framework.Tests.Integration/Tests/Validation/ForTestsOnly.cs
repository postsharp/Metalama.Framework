using System;
using Metalama.Framework.Advising;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;
using Metalama.Framework.Validation;

namespace Metalama.Framework.Tests.Integration.Validation.ForTestsOnly
{
    internal class ForTestOnlyAttribute : Aspect, IAspect<IDeclaration>
    {
        private static readonly DiagnosticDefinition<IDeclaration> _error = new(
            "DEMO02",
            Severity.Error,
            "'{0}' can be used only in a namespace whose name ends with '.Tests'." );

        public void BuildAspect( IAspectBuilder<IDeclaration> builder )
        {
            builder.Outbound.ValidateInboundReferences( ValidateReference, ReferenceGranularity.ParameterOrAttribute, ReferenceKinds.All );
        }

        private void ValidateReference( ReferenceValidationContext context )
        {
            if (!context.Origin.Namespace.Name.EndsWith( ".Tests" ))
            {
                context.Diagnostics.Report( _error.WithArguments( context.Destination.Declaration ) );
            }
        }

        public void BuildEligibility( IEligibilityBuilder<IDeclaration> builder ) { }
    }

    internal class Program
    {
        [ForTestOnly]
        public static void MyMethod( string arg )
        {
            // Some very typical business code.
            Console.WriteLine( "Hello, World!" );
        }

        private static void TestMain()
        {
            MyMethod( "Ok" );
        }
    }

    namespace Tests
    {
        internal class TestClas
        {
            private static void TestMain()
            {
                Program.MyMethod( "KO" );
            }
        }
    }
}