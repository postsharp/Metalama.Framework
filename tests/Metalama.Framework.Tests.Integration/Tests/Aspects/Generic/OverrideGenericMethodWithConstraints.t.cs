class TargetCode : Base
{
  [Aspect]
  T MethodWithTypeConstraint<T>(T a)
    where T : IDisposable
  {
    return a;
  }
  [Aspect]
  T MethodWithConstructorConstraint<T>(T a)
    where T : IDisposable, new()
  {
    return a;
  }
  [Aspect]
  T MethodWithStructConstraint<T>(T a)
    where T : struct, IDisposable
  {
    return a;
  }
  [Aspect]
  T MethodWithNotNullConstraint<T>(T a)
    where T : notnull, IDisposable
  {
    return a;
  }
  [Aspect]
  T MethodWithUnmanagedConstraint<T>(T a)
    where T : unmanaged
  {
    return a;
  }
  [Aspect]
  T MethodWithClassConstraint<T>(T a)
    where T : class, IDisposable
  {
    return a;
  }
  [Aspect]
  T MethodWithNullableClassConstraint<T>(T a)
    where T : class?, IDisposable
  {
    return a;
  }
  [Aspect]
  public override void VirtualMethod<T>()
    where T : default
  {
    return;
  }
}