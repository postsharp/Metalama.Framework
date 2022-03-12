// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'BaseType' in declaration 'DerivedClass' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'FieldType' in declaration 'DerivedClass._field1' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass._field2' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ReturnType, NullableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ParameterType, ArrayType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param1' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeArgument, ParameterType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param2' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'LocalVariableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=IdentifierName).`
// Warning MY001 on `new()`: `Reference constraint of type 'ObjectCreation' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=ImplicitObjectCreationExpression).`
// Warning MY001 on `var`: `Reference constraint of type 'LocalVariableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=IdentifierName).`
// Warning MY001 on `new ValidatedClass()`: `Reference constraint of type 'ObjectCreation' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=ObjectCreationExpression).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'Other' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=IdentifierName).`
// Warning MY001 on `InstanceField`: `Reference constraint of type 'Other' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=IdentifierName).`
// Warning MY001 on `x.InstanceField`: `Reference constraint of type 'Assignment' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=SimpleMemberAccessExpression).`
// Warning MY001 on `x.InstanceField`: `Reference constraint of type 'Assignment' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=SimpleMemberAccessExpression).`
// Warning MY001 on `ValidatedClass.StaticField`: `Reference constraint of type 'Assignment' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=SimpleMemberAccessExpression).`
// Warning MY001 on `Method`: `Reference constraint of type 'Invocation' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'LocalVariableType' in declaration 'ReferencingClass.ReferencingMethod()' (SyntaxKind=IdentifierName).`
// Warning MY001 on `ValidatedClass.Method`: `Reference constraint of type 'Invocation' in declaration 'ReferencingClass.ReferencingMethod()' (SyntaxKind=SimpleMemberAccessExpression).`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'ReferencingClass.ReferencingMethod()' (SyntaxKind=IdentifierName).`
internal class DerivedClass : ValidatedClass
    {
        // Field type.
        private ValidatedClass _field1;

        // Typeof in field initializer.
        private Type _field2 = typeof(ValidatedClass);

        private ValidatedClass? Method( ValidatedClass[] param1, List<ValidatedClass> param2 )
        {
            ValidatedClass variable = new();
            var x = new ValidatedClass();
            _ = x.InstanceField;
            x.InstanceField = 5;
            x.InstanceField += 5;
            ValidatedClass.StaticField = 5;
            Method( typeof(ValidatedClass) );

            return null;
        }
    }
