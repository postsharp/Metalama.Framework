[Test]
public class TestClass
{
  public IEnumerable? Enumerable(string text)
  {
    var returnValue = global::Metalama.Framework.RunTime.RunTimeAspectHelper.Buffer(this.Enumerable_Source(text));
    global::System.Console.WriteLine("Advice 1");
    foreach (var item in returnValue!)
    {
      if (item is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    global::System.Console.WriteLine("Advice 2");
    foreach (var item_1 in returnValue!)
    {
      if (item_1 is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    return returnValue;
  }
  private IEnumerable? Enumerable_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
  public IEnumerator Enumerator(string text)
  {
    var returnValue = global::Metalama.Framework.RunTime.RunTimeAspectHelper.Buffer(this.Enumerator_Source(text));
    var contractEnumerator = returnValue;
    global::System.Console.WriteLine("Advice 1");
    while (contractEnumerator.MoveNext())
    {
      if (contractEnumerator.Current is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    contractEnumerator = returnValue;
    global::System.Console.WriteLine("Advice 2");
    while (contractEnumerator.MoveNext())
    {
      if (contractEnumerator.Current is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    while (returnValue.MoveNext())
    {
      yield return returnValue.Current;
    }
  }
  private IEnumerator Enumerator_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
  public IEnumerable<string> EnumerableT(string text)
  {
    var returnValue = global::Metalama.Framework.RunTime.RunTimeAspectHelper.Buffer(this.EnumerableT_Source(text));
    global::System.Console.WriteLine("Advice 1");
    foreach (var item in returnValue)
    {
      if (item is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    global::System.Console.WriteLine("Advice 2");
    foreach (var item_1 in returnValue)
    {
      if (item_1 is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    return returnValue;
  }
  private IEnumerable<string> EnumerableT_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
  public IEnumerator<string> EnumeratorT(string text)
  {
    var returnValue = global::Metalama.Framework.RunTime.RunTimeAspectHelper.Buffer(this.EnumeratorT_Source(text));
    var contractEnumerator = returnValue;
    global::System.Console.WriteLine("Advice 1");
    while (contractEnumerator.MoveNext())
    {
      if (contractEnumerator.Current is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    contractEnumerator = returnValue;
    global::System.Console.WriteLine("Advice 2");
    while (contractEnumerator.MoveNext())
    {
      if (contractEnumerator.Current is null)
      {
        throw new global::System.ArgumentNullException("<return>");
      }
    }
    while (returnValue.MoveNext())
    {
      yield return returnValue.Current;
    }
  }
  private IEnumerator<string> EnumeratorT_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
}