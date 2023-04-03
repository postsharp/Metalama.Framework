class TargetCode
{
  class Nullable
  {
    [Aspect]
    void ValueType(int i)
    {
      i.ToString();
      return;
    }
    [Aspect]
    void NullableValueType(int? i)
    {
      i?.ToString();
      return;
    }
    [Aspect]
    void ReferenceType(string s)
    {
      s.ToString();
      return;
    }
    [Aspect]
    void NullableReferenceType(string? s)
    {
      s?.ToString();
      return;
    }
    [Aspect]
    void Generic<T>(T t)
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void NullableGeneric<T>(T? t)
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void NotNullGeneric<T>(T t)
      where T : notnull
    {
      t.ToString();
      return;
    }
    [Aspect]
    void NullableNotNullGeneric<T>(T? t)
      where T : notnull
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void ValueTypeGeneric<T>(T t)
      where T : struct
    {
      t.ToString();
      return;
    }
    [Aspect]
    void NullableValueTypeGeneric<T>(T? t)
      where T : struct
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void ReferenceTypeGeneric<T>(T t)
      where T : class
    {
      t.ToString();
      return;
    }
    [Aspect]
    void NullableReferenceTypeGeneric<T>(T? t)
      where T : class
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void ReferenceTypeNullableGeneric<T>(T t)
      where T : class?
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void NullableReferenceTypeNullableGeneric<T>(T? t)
      where T : class?
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void SpecificReferenceTypeGeneric<T>(T t)
      where T : IComparable
    {
      t.ToString();
      return;
    }
    [Aspect]
    void SpecificNullableReferenceTypeGeneric<T>(T? t)
      where T : IComparable
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void SpecificReferenceTypeNullableGeneric<T>(T t)
      where T : IComparable?
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void SpecificNullableReferenceTypeNullableGeneric<T>(T? t)
      where T : IComparable?
    {
      t?.ToString();
      return;
    }
  }
#nullable disable
  class NonNullable
  {
    [Aspect]
    void ValueType(int i)
    {
      i.ToString();
      return;
    }
    [Aspect]
    void NullableValueType(int? i)
    {
      i?.ToString();
      return;
    }
    [Aspect]
    void ReferenceType(string s)
    {
      s?.ToString();
      return;
    }
    [Aspect]
    void Generic<T>(T t)
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void ValueTypeGeneric<T>(T t)
      where T : struct
    {
      t.ToString();
      return;
    }
    [Aspect]
    void NullableValueTypeGeneric<T>(T? t)
      where T : struct
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void ReferenceTypeGeneric<T>(T t)
      where T : class
    {
      t?.ToString();
      return;
    }
    [Aspect]
    void SpecificReferenceTypeGeneric<T>(T t)
      where T : IComparable
    {
      t?.ToString();
      return;
    }
  }
}