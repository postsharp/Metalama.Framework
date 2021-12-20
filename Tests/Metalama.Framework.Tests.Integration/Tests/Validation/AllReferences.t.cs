// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'BaseType' in declaration 'DerivedClass'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'FieldType' in declaration 'DerivedClass._field1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass._field2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ReturnType, NullableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ParameterType, ArrayType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeArgument, ParameterType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'LocalVariableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `Method`: `Reference constraint of type 'MemberAccess' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
using System;
using System.Collections.Generic;
using Metalama.Framework;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Validation;
using Metalama.Framework.Diagnostics;

#pragma warning disable CS0168,CS8618,CS0169


namespace Metalama.Framework.Tests.Integration.Validation.AllReferences
{
#pragma warning disable CS0067
    class Aspect : TypeAspect
    {
    private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning =
            new ( "MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'." );
                
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.WithTarget().RegisterReferenceValidator(
             nameof(Validate),
             ReferenceKinds.All );
        }
        
     private static void Validate(in ReferenceValidationContext context) => throw new System.NotSupportedException("Compile-time only code cannot be called at run-time.");

    }
#pragma warning restore CS0067

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