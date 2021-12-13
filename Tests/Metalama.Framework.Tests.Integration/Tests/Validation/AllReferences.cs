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
    private static readonly DiagnosticDefinition<(ValidatedReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
            new ( "MY001", Severity.Warning, "Reference constraint of type {0} in declaration {1}." );
                
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.WithTarget().AddSourceReferenceValidator(
             nameof(Validate),
             ValidatedReferenceKinds.All );
        }
        
     private static void Validate( in ValidateReferenceContext context )
     {
        context.Diagnostics.Report( context.DiagnosticLocation, _warning, ( context.ReferenceKinds, context.ReferencingDeclaration  ) );
     }
    }

    [Aspect]
    class ValidatedClass
    {
       
        
    }
    
    
    // Base type.
    class DerivedClass : ValidatedClass
    {
        // Field type.
        ValidatedClass _field1;
        
        // Typeof in field initializer.
        Type _field2 = typeof(ValidatedClass);
        
        
        ValidatedClass? Method( ValidatedClass[] param1, List<ValidatedClass> param2 )
        {
            return null;
        }
    }

    
}