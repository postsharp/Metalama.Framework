[Test]
public class TestClass
{
  public IEnumerable Enumerable(string text)
  {
    global::System.Console.WriteLine($"Advice");
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    return this.Enumerable_Source(text);
  }
  private IEnumerable Enumerable_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
  public IEnumerator Enumerator(string text)
  {
    global::System.Console.WriteLine($"Advice");
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    return this.Enumerator_Source(text);
  }
  private IEnumerator Enumerator_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
  public IEnumerable<string> EnumerableT(string text)
  {
    global::System.Console.WriteLine($"Advice");
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    return this.EnumerableT_Source(text);
  }
  private IEnumerable<string> EnumerableT_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
  public IEnumerator<string> EnumeratorT(string text)
  {
    global::System.Console.WriteLine($"Advice");
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    return this.EnumeratorT_Source(text);
  }
  private IEnumerator<string> EnumeratorT_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
}