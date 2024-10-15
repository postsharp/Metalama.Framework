// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'BaseType' in declaration 'DerivedClass'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'MemberType' in declaration 'DerivedClass._field1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'DerivedClass._field2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ReturnType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'ArrayElementType' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param1'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeArgument' in declaration 'DerivedClass.Method(ValidatedClass[], List<ValidatedClass>)/param2'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'LocalVariableType' in declaration 'ReferencingClass.Method()'.`
// Warning MY001 on `Method`: `Reference constraint of type 'Invocation' in declaration 'ReferencingClass.Method()'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'TypeOf' in declaration 'ReferencingClass.Method()'.`
using System;
using System.Collections.Generic;
#pragma warning disable CS0168, CS8618, CS0169
namespace Metalama.Framework.Tests.AspectTests.Validation.CrossAssembly
{
  // Base type.
  internal class DerivedClass : ValidatedClass
  {
    // Field type.
    private ValidatedClass _field1;
    // Typeof in field initializer.
    private Type _field2 = typeof(ValidatedClass);
    private ValidatedClass? Method(ValidatedClass[] param1, List<ValidatedClass> param2)
    {
      return null;
    }
  }
  internal class ReferencingClass
  {
    private void Method()
    {
      ValidatedClass variable;
      ValidatedClass.Method(typeof(ValidatedClass));
    }
  }
}