using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using Metalama.Framework.Diagnostics;

#pragma warning disable CS0168, CS8618, CS0169


namespace Metalama.Framework.Tests.Integration.Validation.AllReferences
{
    class Aspect : TypeAspect
    {
    private static readonly DiagnosticDefinition<(ValidatedReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
            new ( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'." );
                
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.WithTarget().RegisterReferenceValidator(
             nameof(Validate),
             ValidatedReferenceKinds.All );
        }
        
     private static void Validate( in ReferenceValidationContext context )
     {
        context.Diagnostics.Report( context.DiagnosticLocation, _warning, ( context.ReferenceKinds, context.ReferencingDeclaration  ) );
     }
    }

    [Aspect]
    class ValidatedClass
    {
       public static void Method( object o ) {}
        
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
            ValidatedClass variable;
            ValidatedClass.Method( typeof(ValidatedClass) );
            return null;
        }
    }

    
}