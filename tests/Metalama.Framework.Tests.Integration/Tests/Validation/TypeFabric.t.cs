// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'BaseType' in declaration 'DerivedClass'.`
// Warning MY001 on `ValidatedClass`: `Reference constraint of type 'MemberType, NullableType' in declaration 'DerivedClass._field1'.`
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
using System;
using System.Collections.Generic;
using Metalama.Framework.Code;
using Metalama.Framework.Diagnostics;
using Metalama.Framework.Fabrics;
using Metalama.Framework.Validation;
#pragma warning disable CS0168, CS8618, CS0169
namespace Metalama.Framework.Tests.Integration.Validation.TypeFabric_
{
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  internal class ValidatedClass
  {
    public static void Method(object o)
    {
    }
#pragma warning disable CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
    private class Fabric : TypeFabric
    {
      private static readonly DiagnosticDefinition<(ReferenceKinds ReferenceKinds, IDeclaration Declaration)> _warning = new("MY001", Severity.Warning, "Reference constraint of type '{0}' in declaration '{1}'.");
      public override void AmendType(ITypeAmender amender) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
      private static void Validate(ReferenceValidationContext context) => throw new System.NotSupportedException("Compile-time-only code cannot be called at run-time.");
    }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  }
#pragma warning restore CS0067, CS8618, CS0162, CS0169, CS0414, CA1822, CA1823, IDE0051, IDE0052
  internal class DerivedClass : ValidatedClass
  {
    // Field type.
    private ValidatedClass? _field1;
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
}