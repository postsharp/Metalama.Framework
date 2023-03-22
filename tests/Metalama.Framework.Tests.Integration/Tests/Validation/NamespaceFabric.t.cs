// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'BaseType' in declaration 'DerivedClass'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'MemberType' in declaration 'DerivedClass._field1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass._field2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ReturnType, NullableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ParameterType, ArrayType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeArgument, ParameterType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'LocalVariableType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `Method`: `Reference constraint of type 'Invocation' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'LocalVariableType' in declaration 'ReferencingClass.ReferencingMethod()'.`
// Warning MY001 on `ValidatedClass.Method`: `Reference constraint of type 'Invocation' in declaration 'ReferencingClass.ReferencingMethod()'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'ReferencingClass.ReferencingMethod()'.`
internal class ValidatedClass
{
  public static void Method(object o)
  {
  }
}
internal class DerivedClass : ValidatedClass
{
  // Field type.
  private ValidatedClass _field1;
  // Typeof in field initializer.
  private Type _field2 = typeof(ValidatedClass);
  private ValidatedClass? Method(ValidatedClass[] param1, List<ValidatedClass> param2)
  {
    ValidatedClass variable;
    Method(typeof(ValidatedClass));
    return null;
  }
}
internal class ReferencingClass
{
  private void ReferencingMethod()
  {
    ValidatedClass variable;
    ValidatedClass.Method(typeof(ValidatedClass));
  }
}