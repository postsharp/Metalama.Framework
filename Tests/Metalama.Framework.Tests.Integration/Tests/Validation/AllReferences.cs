using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Validation.AllReferences
{
    class Aspect : TypeAspect
    {
    private static readonly DiagnosticDefinition<(ValidatedReferenceKinds ReferenceKinds, string SyntaxKind)> _warning =
            new ( "MY001", Severity.Warning, "Reference constraint of type {0} on type {1}." );
                
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            base.BuildAspect(builder);
            builder.WithTarget().AddSourceReferenceValidator(
             nameof(Validate),
             ValidatedReferenceKinds.All );
        }
        
     private static void Validate( in ValidateReferenceContext context )
     {
        context.Diagnostics.Report( context.DiagnosticLocation, _warning, ( context.ReferenceKinds, context.Syntax.Kind  ) );
     }
    }

    [Aspect]
    class ValidatedClass
    {
       
        
    }
    
    class DerivedClass : ValidatedClass
    {
    }
    
    
}