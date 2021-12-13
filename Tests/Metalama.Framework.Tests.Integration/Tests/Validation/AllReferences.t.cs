// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'BaseType' in declaration 'DerivedClass'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'FieldType' in declaration 'DerivedClass._field1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass._field2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ReturnType, NullableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ParameterType, ArrayType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeArgument, ParameterType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'LocalVariableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning CS0168 on `variable`: `The variable 'variable' is declared but never used`
using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using Metalama.Framework.Diagnostics;

namespace Metalama.Framework.Tests.Integration.Validation.AllReferences
{
#pragma warning disable CS0067
    class Aspect : TypeAspect
    {
    private static readonly DiagnosticDefinition<(ValidatedReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
            new ( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'." );
                
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.WithTarget().AddSourceReferenceValidator(
             nameof(Validate),
             ValidatedReferenceKinds.All );
        }
        
     private static void Validate(in ValidateReferenceContext context) => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

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
            ValidatedClass variable;
            return null;
        }
    }

    
}
