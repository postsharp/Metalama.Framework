[ReturnNumbers]
private object Method()
{
  global::System.Collections.Generic.IEnumerable<global::System.Object>? numbers = new object[]
  {
    42
  };
  numbers = numbers is { } iEnumerable ? global::System.Linq.Enumerable.ToList(iEnumerable) : null;
  numbers = numbers is { } iEnumerable_1 ? global::System.Linq.Enumerable.ToList(iEnumerable_1) : null;
  return (global::System.Object)numbers;
}