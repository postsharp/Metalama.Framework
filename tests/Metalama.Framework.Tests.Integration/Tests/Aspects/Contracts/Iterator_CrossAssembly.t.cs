[Test]
public class TestClass
{
  public IEnumerable<string> Enumerable(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    foreach (var returnItem in this.Enumerable_Source(text))
    {
      yield return returnItem;
    }
  }
  private IEnumerable<string> Enumerable_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
  public IEnumerator<string> Enumerator(string text)
  {
    if (text is null)
    {
      throw new global::System.ArgumentNullException("text");
    }
    var returnEnumerator = this.Enumerator_Source(text);
    while (returnEnumerator.MoveNext())
    {
      yield return returnEnumerator.Current;
    }
  }
  private IEnumerator<string> Enumerator_Source(string text)
  {
    yield return "Hello";
    yield return text;
  }
}