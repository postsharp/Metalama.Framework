class TargetCode
{
  [Aspect]
  T Method<T>(T a)
  {
    return a;
  }
}