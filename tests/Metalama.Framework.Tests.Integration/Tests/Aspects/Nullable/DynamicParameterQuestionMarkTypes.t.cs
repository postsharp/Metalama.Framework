class TargetCode
{
  class Nullable
  {
    [Aspect]
    void ValueType(int arg)
    {
      arg.ToString();
    }
    [Aspect]
    void NullableValueType(int? arg)
    {
      arg?.ToString();
    }
    [Aspect]
    void ReferenceType(string arg)
    {
      arg.ToString();
    }
    [Aspect]
    void NullableReferenceType(string? arg)
    {
      arg?.ToString();
    }
    [Aspect]
    void Generic<T>(T arg)
    {
      arg?.ToString();
    }
    [Aspect]
    void NullableGeneric<T>(T? arg)
    {
      arg?.ToString();
    }
    [Aspect]
    void NotNullGeneric<T>(T arg)
      where T : notnull
    {
      arg.ToString();
    }
    [Aspect]
    void NullableNotNullGeneric<T>(T? arg)
      where T : notnull
    {
      arg?.ToString();
    }
    [Aspect]
    void ValueTypeGeneric<T>(T arg)
      where T : struct
    {
      arg.ToString();
    }
    [Aspect]
    void NullableValueTypeGeneric<T>(T? arg)
      where T : struct
    {
      arg?.ToString();
    }
    [Aspect]
    void ReferenceTypeGeneric<T>(T arg)
      where T : class
    {
      arg.ToString();
    }
    [Aspect]
    void NullableReferenceTypeGeneric<T>(T? arg)
      where T : class
    {
      arg?.ToString();
    }
    [Aspect]
    void ReferenceTypeNullableGeneric<T>(T arg)
      where T : class?
    {
      arg?.ToString();
    }
    [Aspect]
    void NullableReferenceTypeNullableGeneric<T>(T? arg)
      where T : class?
    {
      arg?.ToString();
    }
    [Aspect]
    void SpecificReferenceTypeGeneric<T>(T arg)
      where T : IComparable
    {
      arg.ToString();
    }
    [Aspect]
    void SpecificNullableReferenceTypeGeneric<T>(T? arg)
      where T : IComparable
    {
      arg?.ToString();
    }
    [Aspect]
    void SpecificReferenceTypeNullableGeneric<T>(T arg)
      where T : IComparable?
    {
      arg?.ToString();
    }
    [Aspect]
    void SpecificNullableReferenceTypeNullableGeneric<T>(T? arg)
      where T : IComparable?
    {
      arg?.ToString();
    }
  }
#nullable disable
  class NonNullable
  {
    [Aspect]
    void ValueType(int arg)
    {
      arg.ToString();
    }
    [Aspect]
    void NullableValueType(int? arg)
    {
      arg?.ToString();
    }
    [Aspect]
    void ReferenceType(string arg)
    {
      arg?.ToString();
    }
    [Aspect]
    void Generic<T>(T arg)
    {
      arg?.ToString();
    }
    [Aspect]
    void ValueTypeGeneric<T>(T arg)
      where T : struct
    {
      arg.ToString();
    }
    [Aspect]
    void NullableValueTypeGeneric<T>(T? arg)
      where T : struct
    {
      arg?.ToString();
    }
    [Aspect]
    void ReferenceTypeGeneric<T>(T arg)
      where T : class
    {
      arg?.ToString();
    }
    [Aspect]
    void SpecificReferenceTypeGeneric<T>(T arg)
      where T : IComparable
    {
      arg?.ToString();
    }
  }
}