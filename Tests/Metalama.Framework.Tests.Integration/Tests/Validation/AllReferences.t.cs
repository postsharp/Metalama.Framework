// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'BaseType' in declaration 'DerivedClass'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'FieldType' in declaration 'DerivedClass._field1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass._field2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ReturnType, NullableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ParameterType, ArrayType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeArgument, ParameterType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'LocalVariableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `Method`: `Reference constraint of type 'MemberAccess' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
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
