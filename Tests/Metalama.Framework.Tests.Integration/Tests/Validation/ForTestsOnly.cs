using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Eligibility;

namespace Metalama.Framework.Tests.Integration.Validation.ForTestsOnly
{
    
    class ForTestOnlyAttribute : Aspect, IAspect<IDeclaration>
    {
        private static readonly DiagnosticDefinition<IDeclaration> _warning = new(
            "DEMO02",
            Severity.Error,
            "'{0}' can be used only in a namespace whose name ends with '.Tests'." );
    
        public void BuildAspect( IAspectBuilder<IDeclaration> builder )
        {
            builder.WithTarget().RegisterReferenceValidator( this.ValidateReference, ReferenceKinds.All );
        }
    
        private void ValidateReference( in ReferenceValidationContext context )
        {
            if ( !context.ReferencingType.Namespace.Name.EndsWith(".Tests"))
            {
                context.Diagnostics.Report( _warning.WithArguments( context.ReferencedDeclaration ) );
            }
        }

        public void BuildEligibility( IEligibilityBuilder<IDeclaration> builder )
        {
            
        }
    }
    
    class Program
    {
        [ForTestOnly]
        public static void MyMethod(string arg)
        {
            // Some very typical business code.
            Console.WriteLine("Hello, World!");
        }
    
        static void Main()
        {
            MyMethod("Ok");
        }
    }
    
    namespace Tests
    {
        class TestClas
        {
            
        static void Main()
        {
            Program.MyMethod("KO");
        }
        }
    }
    
    


}